using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using VKore.Core;
using VKore.Infrastructure.Logging;

namespace VKore.VK_Services;

public class MessageMonitor
{
    private readonly BotContext _ctx;
    private readonly ITelegramBotClient _tg;

    private readonly ConcurrentDictionary<long, CachedMsg> _cache = new();
    private readonly ConcurrentDictionary<long, string> _names = new();

    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(35),
        DefaultRequestHeaders = { { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) VKore/1.0" } }
    };

    private record CachedMsg(long Id, long SenderId, string SenderName, string Text, DateTime At);

    public MessageMonitor(BotContext ctx, ITelegramBotClient tg)
    {
        _ctx = ctx;
        _tg  = tg;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        AppLogger.Log("LongPoll: инициализация...");

        var lp     = _ctx.VkApi.Messages.GetLongPollServer(needPts: false, lpVersion: 3u);
        var ts     = lp.Ts.ToString();
        var key    = lp.Key;
        var server = FixServer(lp.Server);

        AppLogger.Log("LongPoll: подключён, жду события.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var url  = $"{server}?act=a_check&key={key}&ts={ts}&wait=25&mode=2&version=3";
                var raw  = await _http.GetStringAsync(url, ct);
                var json = JObject.Parse(raw);

                if (json["failed"] is { } err)
                {
                    if (err.Value<int>() == 1)
                        ts = json["ts"]?.Value<string>() ?? ts;
                    else
                    {
                        var fresh = _ctx.VkApi.Messages.GetLongPollServer(needPts: false, lpVersion: 3u);
                        ts = fresh.Ts.ToString(); key = fresh.Key; server = FixServer(fresh.Server);
                    }
                    continue;
                }

                ts = json["ts"]?.Value<string>() ?? ts;

                if (json["updates"] is not JArray updates) continue;

                foreach (var ev in updates)
                    await HandleEvent(ev, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (HttpRequestException) { await Task.Delay(5000, ct); }
            catch (Exception ex)
            {
                AppLogger.Log($"LongPoll: {ex.Message}", LogType.Warning);
                await Task.Delay(2000, ct);
            }
        }
    }

    private async Task HandleEvent(JToken ev, CancellationToken ct)
    {
        int code = ev[0]?.Value<int>() ?? -1;

        if (code == 4) // новое сообщение
        {
            var msgId  = ev[1]!.Value<long>();
            var flags  = ev[2]!.Value<int>();
            var peerId = ev[3]!.Value<long>();
            var text   = ev[5]!.Value<string>() ?? string.Empty;

            if ((flags & 2) != 0) return; // пропускаем свои

            var senderId = peerId;
            if (peerId > 2_000_000_000 && ev[6] is JObject extra && extra["from"] != null)
                senderId = extra["from"]!.Value<long>();

            var name = await ResolveName(senderId);
            _cache[msgId] = new CachedMsg(msgId, senderId, name, text, DateTime.UtcNow);

            AppLogger.Log($"💬 {name}: {text}", LogType.System);
            TrimCache();

            if (_ctx.Config.NotifyOnNewMessages)
                await Notify($"📬 *Новое сообщение*\n👤 *{Esc(name)}*\n💬 {Esc(text)}", ct);
        }
        else if (code == 2)
        {
            var msgId = ev[1]!.Value<long>();
            var flagsSet = ev[2]!.Value<int>();

            // флаг 128 = сообщение удалено
            if ((flagsSet & 128) == 0) return;
            if (!_cache.TryRemove(msgId, out var m)) return;

            AppLogger.Log($"🚨 Удалено от {m.SenderName}: {m.Text}", LogType.Warning);

            if (_ctx.Config.NotifyOnDeletedMessages)
                await Notify($"🚨 *Удалённое сообщение*\n👤 *{Esc(m.SenderName)}*\n💬 {Esc(m.Text)}", ct);
        }
    }

    private async Task Notify(string text, CancellationToken ct)
    {
        try
        {
            await _tg.SendMessage(_ctx.Config.TelegramUserId, text,
                parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            AppLogger.Log($"[TG] Ошибка отправки: {ex.Message}", LogType.Warning);
        }
    }

    private async Task<string> ResolveName(long id)
    {
        if (_names.TryGetValue(id, out var cached)) return cached;

        try
        {
            string name;
            if (id < 0)
            {
                var g = await _ctx.VkApi.Groups.GetByIdAsync(null, Math.Abs(id).ToString(), null);
                name = g?.FirstOrDefault()?.Name ?? $"id{id}";
            }
            else
            {
                var u = await _ctx.VkApi.Users.GetAsync(new[] { id });
                var user = u?.FirstOrDefault();
                name = user != null ? $"{user.FirstName} {user.LastName}" : $"id{id}";
            }

            _names[id] = name;
            return name;
        }
        catch
        {
            return $"id{id}";
        }
    }

    private void TrimCache()
    {
        if (_cache.Count <= 2000) return;

        var old = _cache.Values
            .OrderBy(m => m.At)
            .Take(200)
            .Select(m => m.Id);

        foreach (var k in old) _cache.TryRemove(k, out _);
    }

    private static string FixServer(string s) =>
        s.StartsWith("http") ? s : "https://" + s;

    private static string Esc(string s) =>
        s.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace("[", "\\[");
}
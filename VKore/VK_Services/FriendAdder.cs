using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;
using VKore.Core;
using VKore.Infrastructure.Logging;

namespace VKore.VK_Services;

public class FriendAdder
{
    private readonly BotContext _ctx;
    private readonly ITelegramBotClient _tg;

    private int _addedToday;
    private DateTime _dayMark = DateTime.UtcNow.Date;

    public FriendAdder(BotContext ctx, ITelegramBotClient tg)
    {
        _ctx = ctx;
        _tg  = tg;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        AppLogger.Log("FriendAdder: запущен.");

        while (!ct.IsCancellationRequested)
        {
            ResetCounterIfNewDay();

            if (!_ctx.Config.FriendAutoAddEnabled)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), ct);
                continue;
            }

            if (_ctx.Me.BirthDate == null)
            {
                AppLogger.Log("FriendAdder: у тебя не указана дата рождения в VK — фильтр по возрасту невозможен.", LogType.Warning);
                await Task.Delay(TimeSpan.FromHours(1), ct);
                continue;
            }

            if (_addedToday >= _ctx.Config.FriendsPerDay)
            {
                var sleepUntil = _dayMark.AddDays(1);
                var delay      = sleepUntil - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    AppLogger.Log($"FriendAdder: лимит на сегодня достигнут. Следующий запуск через {(int)delay.TotalHours}ч {delay.Minutes}м.");
                    await Task.Delay(delay, ct);
                }
                continue;
            }

            try
            {
                await ProcessBatch(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                AppLogger.Log($"FriendAdder: {ex.Message}", LogType.Warning);
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }

            await Task.Delay(TimeSpan.FromMinutes(30), ct);
        }
    }

    private async Task ProcessBatch(CancellationToken ct)
    {
        var api = _ctx.VkApi;
        var cfg = _ctx.Config;
        var me  = _ctx.Me;

        AppLogger.Log("FriendAdder: ищу кандидатов...");

        var suggestions = api.Friends.GetSuggestions(
            fields: UsersFields.BirthDate | UsersFields.CommonCount,
            count: 100,
            offset: 0
        );

        if (suggestions == null || suggestions.Count == 0)
        {
            AppLogger.Log("FriendAdder: нет предложений от VK.");
            return;
        }

        foreach (var candidate in suggestions)
        {
            if (ct.IsCancellationRequested) break;
            if (_addedToday >= cfg.FriendsPerDay) break;

            var candidateBirth = ParseBirthDate(candidate.BirthDate);
            if (candidateBirth == null)
                continue;

            var ageDiff = Math.Abs(me.BirthDate!.Value.Year - candidateBirth.Value.Year);
            if (ageDiff > 5)
                continue;

            if (candidate.CommonCount == null || candidate.CommonCount == 0)
                continue;

            var mutualFriendName = await CheckMutualInteraction(candidate.Id, ct);
            if (mutualFriendName == null)
                continue;

            await HumanDelay.WaitAsync(cfg);

            try
            {
                api.Friends.Add(userId: candidate.Id, text: string.Empty, follow: false);
                _addedToday++;

                var fullName = $"{candidate.FirstName} {candidate.LastName}";
                AppLogger.Log($"FriendAdder: добавил {fullName} ({_addedToday}/{cfg.FriendsPerDay}).", LogType.System);

                await SendNotify(fullName, mutualFriendName, ct);
            }
            catch (Exception ex)
            {
                AppLogger.Log($"FriendAdder: не удалось добавить {candidate.Id} — {ex.Message}", LogType.Warning);
            }

            await HumanDelay.WaitAsync(cfg);
        }
    }

    private async Task<string?> CheckMutualInteraction(long candidateId, CancellationToken ct)
    {
        try
        {
            await Task.Yield();

            var mutualResult = _ctx.VkApi.Friends.GetMutual(new FriendsGetMutualParams
            {
                TargetUid = candidateId,
                SourceUid = _ctx.Me.Id,
                Count     = 1,
            });

            if (mutualResult == null || mutualResult.Count == 0)
                return null;

            var commonFriendId = (long)mutualResult[0].Id;

            var history = _ctx.VkApi.Messages.GetHistory(new MessagesGetHistoryParams
            {
                UserId = commonFriendId,
                Count  = 20,
            });

            if (history?.Messages == null || !history.Messages.Any())
                return null;

            var hasContact = history.Messages.Any(m =>
                m.FromId.GetValueOrDefault() == candidateId ||
                m.PeerId.GetValueOrDefault() == candidateId);

            if (!hasContact)
                return null;

            var friendProfile = _ctx.VkApi.Users.Get(new[] { commonFriendId });
            var friend        = friendProfile?.FirstOrDefault();

            return friend != null
                ? $"{friend.FirstName} {friend.LastName}"
                : $"id{commonFriendId}";
        }
        catch
        {
            return null;
        }
    }

    private async Task SendNotify(string candidateName, string? mutualFriendName, CancellationToken ct)
    {
        try
        {
            var reason = mutualFriendName != null
                ? $"был контакт с *{Esc(mutualFriendName)}* в переписке"
                : "подходит по возрасту, общих друзей нет";

            var text =
                $"👤 Кинул заявку *{Esc(candidateName)}*\n" +
                $"┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\n" +
                $"📋 Потому что {reason}";

            await _tg.SendMessage(
                _ctx.Config.TelegramUserId,
                text,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            AppLogger.Log($"FriendAdder: не смог отправить уведомление — {ex.Message}", LogType.Warning);
        }
    }

    private static DateTime? ParseBirthDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var parts = raw.Split('.');
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out int d) &&
            int.TryParse(parts[1], out int m) &&
            int.TryParse(parts[2], out int y))
        {
            return new DateTime(y, m, d);
        }

        return null;
    }

    private void ResetCounterIfNewDay()
    {
        var today = DateTime.UtcNow.Date;
        if (_dayMark == today) return;

        _dayMark    = today;
        _addedToday = 0;
        AppLogger.Log("FriendAdder: новый день, счётчик сброшен.");
    }

    private static string Esc(string s) =>
        s.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace("[", "\\[");
}

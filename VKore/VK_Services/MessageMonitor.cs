using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model;
using VKore.Logger;
using VKore.Options;
using VKore.System;

namespace VKore.VK_Services;

public class MessageMonitor
{
    private static readonly ConcurrentDictionary<long, SavedMessage> MessageCache = new();
    private static readonly ConcurrentDictionary<long, string> NameCache = new();
    
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(35) };

    public struct SavedMessage
    {
        public long MessageId { get; set; }
        public long PeerId { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public static async Task StartAsync(VkApi api)
    {
        Logger.Logger.Log("Инициализация службы безопасного отслеживания сообщений (LongPoll)...", LogType.System);
        Logger.Logger.Log("Для выхода нажмите Ctrl + C.", LogType.Warning);

        try
        {
            var lpServer = api.Messages.GetLongPollServer(needPts: false, lpVersion: 3u);
            
            string ts = lpServer.Ts.ToString();
            string key = lpServer.Key;
            string server = lpServer.Server;
            
            if (!server.StartsWith("http"))
            {
                server = "https://" + server;
            }

            Logger.Logger.Log("Наблюдатель сообщений успешно запущен через HTTP-туннель!", LogType.System);

            while (true)
            {
                try
                {
                    var url = $"{server}?act=a_check&key={key}&ts={ts}&wait=25&mode=2&version=3";
                    
                    var responseString = await HttpClient.GetStringAsync(url);
                    var json = JObject.Parse(responseString);

                    if (json["failed"] != null)
                    {
                        int failed = json["failed"].Value<int>();
                        if (failed == 1)
                        {
                            ts = json["ts"]?.Value<string>() ?? ts;
                        }
                        else
                        {
                            var newLp = api.Messages.GetLongPollServer(needPts: false, lpVersion: 3u);
                            ts = newLp.Ts.ToString();
                            key = newLp.Key;
                            server = newLp.Server;
                            if (!server.StartsWith("http")) server = "https://" + server;
                        }
                        continue;
                    }

                    ts = json["ts"]?.Value<string>() ?? ts;
                    var updates = json["updates"] as JArray;

                    if (updates == null) continue;

                    foreach (var update in updates)
                    {
                        var eventCode = update[0].Value<int>();

                        switch (eventCode)
                        {
                            case 4:
                            {
                                var messageId = update[1].Value<long>();
                                var flags = update[2].Value<int>();
                                var peerId = update[3].Value<long>();
                                var timestamp = update[4].Value<long>();
                                var text = update[5].Value<string>();

                                var isIncoming = (flags & 2) == 0;
                                if (isIncoming)
                                {
                                    var senderId = peerId;
                                    var attachments = update[6] as JObject;
                                
                                    if (peerId > 2000000000 && attachments != null && attachments["from"] != null)
                                    {
                                        senderId = attachments["from"].Value<long>();
                                    }

                                    var senderName = await GetSenderNameSafeAsync(api, senderId);

                                    var savedMsg = new SavedMessage
                                    {
                                        MessageId = messageId,
                                        PeerId = peerId,
                                        SenderId = senderId,
                                        SenderName = senderName,
                                        Text = text,
                                        Timestamp = DateTime.UtcNow
                                    };

                                    MessageCache[messageId] = savedMsg;

                                    Logger.Logger.Log($"Новое сообщение от {senderName}: {text}", LogType.System);

                                    if (AccountOptions.TelegramForwardingEnabled)
                                    {
                                        var tgMsg = $"📬 *Новое сообщение в VK*\n\n" +
                                                    $"👤 *Отправитель:* {senderName}\n" +
                                                    $"💬 *Текст:* {text}";

                                        await TelegramService.SendMessageAsync(
                                            AccountOptions.TelegramBotToken,
                                            AccountOptions.TelegramChatId,
                                            tgMsg
                                        );
                                    }

                                    PruneCache();
                                }

                                break;
                            }
                            case 2:
                            {
                                long messageId = update[1].Value<long>();
                                int flagsSet = update[2].Value<int>();

                                if ((flagsSet & 128) != 0)
                                {
                                    if (MessageCache.TryRemove(messageId, out var deletedMsg))
                                    {
                                        var consoleLog = $"🚨 ПОЛЬЗОВАТЕЛЬ {deletedMsg.SenderName} (ID: {deletedMsg.SenderId}) УДАЛИЛ СООБЩЕНИЕ: {deletedMsg.Text}";
                                        Logger.Logger.Log(consoleLog, LogType.Warning);

                                        if (AccountOptions.TelegramForwardingEnabled)
                                        {
                                            var tgMsg = $"🚨 *Удалённое сообщение!*\n\n" +
                                                        $"👤 *Отправитель:* {deletedMsg.SenderName} (ID: {deletedMsg.SenderId})\n" +
                                                        $"💬 *Текст:* {deletedMsg.Text}";

                                            await TelegramService.SendMessageAsync(
                                                AccountOptions.TelegramBotToken,
                                                AccountOptions.TelegramChatId,
                                                tgMsg
                                            );
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    await Task.Delay(5000);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log($"Критическая ошибка наблюдателя сообщений: {ex.Message}", LogType.Error);
            Console.ReadKey();
        }
    }
    
    private static async Task<string> GetSenderNameSafeAsync(VkApi api, long senderId)
    {
        if (NameCache.TryGetValue(senderId, out var cachedName))
            return cachedName;

        var name = $"ID: {senderId}";
        try
        {
            if (senderId < 0)
            {
                var groupId = Math.Abs(senderId);
            
                var groups = await api.Groups.GetByIdAsync(null, groupId.ToString(), null);
                if (groups != null && groups.Count > 0)
                {
                    name = groups[0].Name;
                    NameCache[senderId] = name;
                }
            }
            else
            {
                var users = await api.Users.GetAsync(new[] { senderId });
                if (users != null && users.Count > 0)
                {
                    name = $"{users[0].FirstName} {users[0].LastName}";
                    NameCache[senderId] = name;
                }
            }
        }
        catch
        {
            // ignored
        }

        return name;
    }
    
    private static void PruneCache()
    {
        if (MessageCache.Count > 2000)
        {
            var oldestKeys = MessageCache.Values
                .OrderBy(m => m.Timestamp)
                .Take(200)
                .Select(m => m.MessageId)
                .ToList();

            foreach (var key in oldestKeys)
            {
                MessageCache.TryRemove(key, out _);
            }
        }
    }
}
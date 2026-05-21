using VkNet;
using VkNet.Model;
using VKore.Logger;
using VKore.Options;
using VKore.System;

namespace VKore.VK_Services;

public class MessageMonitor
{
    private static readonly HashSet<long> ProcessedMessages = [];

    public static async Task StartAsync(VkApi api)
    {
        Logger.Logger.Log("Инициализация службы отслеживания сообщений...", LogType.System);
        Logger.Logger.Log("Для выхода и возврата в меню нажмите Ctrl + C.", LogType.Warning);

        try
        {
            var initialConversations = api.Messages.GetConversations(new GetConversationsParams { Count = 10 });
            foreach (var item in initialConversations.Items)
            {
                if (item.LastMessage?.Id != null)
                {
                    ProcessedMessages.Add(item.LastMessage.Id.Value);
                }
            }

            Logger.Logger.Log("Наблюдатель сообщений успешно запущен!", LogType.System);

            while (true)
            {
                await BotHelper.DelayRandomlyAsync(20, 60);

                var conversations = api.Messages.GetConversations(new GetConversationsParams { Count = 10 });

                foreach (var item in conversations.Items)
                {
                    var msg = item.LastMessage;

                    if (msg?.Id == null)
                        continue;

                    if (msg.Type == VkNet.Enums.MessageType.Received && !ProcessedMessages.Contains(msg.Id.Value))
                    {
                        ProcessedMessages.Add(msg.Id.Value);

                        var senderName = "ID: " + msg.FromId;
                        try
                        {
                            var user = api.Users.Get(new[] { msg.FromId.Value });
                            if (user.Count > 0)
                            {
                                senderName = $"{user[0].FirstName} {user[0].LastName}";
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        var logMsg = $"Новое сообщение от {senderName}: {msg.Text}";
                        Logger.Logger.Log(logMsg, LogType.System);

                        if (AccountOptions.TelegramForwardingEnabled)
                        {
                            var tgMsg = $"📬 *Новое сообщение в VK*\n\n" +
                                        $"👤 *Отправитель:* {senderName}\n" +
                                        $"💬 *Текст:* {msg.Text}";

                            await TelegramService.SendMessageAsync(
                                AccountOptions.TelegramBotToken,
                                AccountOptions.TelegramChatId,
                                tgMsg
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log($"Критическая ошибка наблюдателя сообщений: {ex.Message}", LogType.Error);
            Console.ReadKey();
        }
    }

}
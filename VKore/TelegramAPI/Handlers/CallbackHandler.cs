using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VKore.Core;
using VKore.Infrastructure.Config;
using VKore.TelegramAPI.Menus;

namespace VKore.TelegramAPI.Handlers;

public class CallbackHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly BotContext _ctx;

    public CallbackHandler(ITelegramBotClient bot, BotContext ctx)
    {
        _bot = bot;
        _ctx = ctx;
    }

    public async Task HandleAsync(CallbackQuery query, CancellationToken ct)
    {
        var data    = query.Data ?? string.Empty;
        var chatId  = query.Message!.Chat.Id;
        var msgId   = query.Message.MessageId;
        var userId  = query.From.Id;
        var cfg     = _ctx.Config;

        await _bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);

        switch (data)
        {
            case "menu:main":
                BotState.Set(userId, MenuScreen.Main);
                await Edit(chatId, msgId, MainMenu.Text, MainMenu.Keyboard, ct);
                break;

            case "menu:settings":
                BotState.Set(userId, MenuScreen.Settings);
                await Edit(chatId, msgId, SettingsMenu.BuildText(cfg), SettingsMenu.BuildKeyboard(cfg), ct);
                break;

            case "menu:notifications":
                BotState.Set(userId, MenuScreen.Notifications);
                await Edit(chatId, msgId, NotificationsMenu.BuildText(cfg), NotificationsMenu.BuildKeyboard(cfg), ct);
                break;

            case "menu:status":
                BotState.Set(userId, MenuScreen.Status);
                await Edit(chatId, msgId, StatusMenu.BuildText(_ctx.Me), StatusMenu.Keyboard, ct);
                break;

            case "menu:about":
                BotState.Set(userId, MenuScreen.About);
                await Edit(chatId, msgId,
                    "ℹ️ *VKore* — Open\\-Source автоматизация ВКонтакте\n\n" +
                    "🔗 [GitHub](https://github.com/Semleks/VKore)\n" +
                    "👨‍💻 Автор: Semlex",
                    new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("◀️ Назад", "menu:main")),
                    ct);
                break;
            
            case "settings:delay":
                BotState.Set(userId, MenuScreen.AwaitingMinDelay);
                await _bot.SendMessage(chatId,
                    $"🕐 Введи минимальную задержку в секундах (сейчас {cfg.MinDelaySeconds}с):",
                    cancellationToken: ct);
                break;

            case "settings:toggle_autolike":
                cfg.AutoLikeEnabled = !cfg.AutoLikeEnabled;
                await ConfigManager.SaveAsync(cfg);
                await Edit(chatId, msgId, SettingsMenu.BuildText(cfg), SettingsMenu.BuildKeyboard(cfg), ct);
                break;

            case "settings:toggle_ai":
                cfg.AiCommentsEnabled = !cfg.AiCommentsEnabled;
                await ConfigManager.SaveAsync(cfg);
                await Edit(chatId, msgId, SettingsMenu.BuildText(cfg), SettingsMenu.BuildKeyboard(cfg), ct);
                break;

            case "settings:toggle_friend_add":
                cfg.FriendAutoAddEnabled = !cfg.FriendAutoAddEnabled;
                await ConfigManager.SaveAsync(cfg);
                await Edit(chatId, msgId, SettingsMenu.BuildText(cfg), SettingsMenu.BuildKeyboard(cfg), ct);
                break;

            case "settings:friends_per_day":
                BotState.Set(userId, MenuScreen.AwaitingFriendsPerDay);
                await _bot.SendMessage(chatId,
                    $"📅 Сколько друзей добавлять в день? Введи число от 1 до 10 (сейчас {cfg.FriendsPerDay}):",
                    cancellationToken: ct);
                break;
            
            case "notif:toggle_new":
                cfg.NotifyOnNewMessages = !cfg.NotifyOnNewMessages;
                await ConfigManager.SaveAsync(cfg);
                await Edit(chatId, msgId, NotificationsMenu.BuildText(cfg), NotificationsMenu.BuildKeyboard(cfg), ct);
                break;

            case "notif:toggle_deleted":
                cfg.NotifyOnDeletedMessages = !cfg.NotifyOnDeletedMessages;
                await ConfigManager.SaveAsync(cfg);
                await Edit(chatId, msgId, NotificationsMenu.BuildText(cfg), NotificationsMenu.BuildKeyboard(cfg), ct);
                break;
        }
    }

    private Task Edit(long chatId, int msgId, string text, InlineKeyboardMarkup kb, CancellationToken ct) =>
        _bot.EditMessageText(chatId, msgId, text,
            parseMode: ParseMode.Markdown,
            replyMarkup: kb,
            cancellationToken: ct);
}
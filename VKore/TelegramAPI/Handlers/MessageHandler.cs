using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VKore.Core;
using VKore.Infrastructure.Config;
using VKore.TelegramAPI.Menus;

namespace VKore.TelegramAPI.Handlers;

public class MessageHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly BotContext _ctx;

    public MessageHandler(ITelegramBotClient bot, BotContext ctx)
    {
        _bot = bot;
        _ctx = ctx;
    }

    public async Task HandleAsync(Message msg, CancellationToken ct)
    {
        var text   = msg.Text ?? string.Empty;
        var chatId = msg.Chat.Id;
        var userId = msg.From?.Id ?? 0;
        var screen = BotState.Get(userId);

        if (screen == MenuScreen.AwaitingMinDelay)
        {
            await HandleMinDelay(chatId, userId, text, ct);
            return;
        }
        if (screen == MenuScreen.AwaitingMaxDelay)
        {
            await HandleMaxDelay(chatId, userId, text, ct);
            return;
        }
        if (screen == MenuScreen.AwaitingFriendsPerDay)
        {
            await HandleFriendsPerDay(chatId, userId, text, ct);
            return;
        }

        if (text.StartsWith("/start"))
        {
            BotState.Set(userId, MenuScreen.Main);
            await _bot.SendMessage(chatId, MainMenu.Text,
                parseMode: ParseMode.Markdown,
                replyMarkup: MainMenu.Keyboard,
                cancellationToken: ct);
        }
    }

    private async Task HandleMinDelay(long chatId, long userId, string input, CancellationToken ct)
    {
        var cfg = _ctx.Config;

        if (!int.TryParse(input.Trim(), out int val) || val <= 0)
        {
            await _bot.SendMessage(chatId, "❌ Нужно целое число больше 0.", cancellationToken: ct);
            return;
        }
        if (val >= cfg.MaxDelaySeconds)
        {
            await _bot.SendMessage(chatId,
                $"❌ Минимум должен быть меньше максимума (сейчас {cfg.MaxDelaySeconds}с).",
                cancellationToken: ct);
            return;
        }

        cfg.MinDelaySeconds = val;
        BotState.Set(userId, MenuScreen.AwaitingMaxDelay);

        await _bot.SendMessage(chatId,
            $"✅ Минимум: *{val}с*. Теперь введи максимум (сейчас {cfg.MaxDelaySeconds}с):",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleMaxDelay(long chatId, long userId, string input, CancellationToken ct)
    {
        var cfg = _ctx.Config;

        if (!int.TryParse(input.Trim(), out int val) || val <= 0)
        {
            await _bot.SendMessage(chatId, "❌ Нужно целое число больше 0.", cancellationToken: ct);
            return;
        }
        if (val <= cfg.MinDelaySeconds)
        {
            await _bot.SendMessage(chatId,
                $"❌ Максимум должен быть больше минимума ({cfg.MinDelaySeconds}с).",
                cancellationToken: ct);
            return;
        }

        cfg.MaxDelaySeconds = val;
        await ConfigManager.SaveAsync(cfg);
        BotState.Set(userId, MenuScreen.Settings);

        await _bot.SendMessage(chatId,
            $"✅ Задержка сохранена: *{cfg.MinDelaySeconds}–{cfg.MaxDelaySeconds}с*",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        await _bot.SendMessage(chatId,
            SettingsMenu.BuildText(cfg),
            parseMode: ParseMode.Markdown,
            replyMarkup: SettingsMenu.BuildKeyboard(cfg),
            cancellationToken: ct);
    }

    private async Task HandleFriendsPerDay(long chatId, long userId, string input, CancellationToken ct)
    {
        var cfg = _ctx.Config;

        if (!int.TryParse(input.Trim(), out int val) || val < 1 || val > 10)
        {
            await _bot.SendMessage(chatId, "❌ Введи число от 1 до 10.", cancellationToken: ct);
            return;
        }

        cfg.FriendsPerDay = val;
        await ConfigManager.SaveAsync(cfg);
        BotState.Set(userId, MenuScreen.Settings);

        await _bot.SendMessage(chatId,
            $"✅ Буду добавлять *{val}* друга(ов) в день.",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        await _bot.SendMessage(chatId,
            SettingsMenu.BuildText(cfg),
            parseMode: ParseMode.Markdown,
            replyMarkup: SettingsMenu.BuildKeyboard(cfg),
            cancellationToken: ct);
    }
}
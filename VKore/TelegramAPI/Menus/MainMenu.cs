using Telegram.Bot.Types.ReplyMarkups;

namespace VKore.TelegramAPI.Menus;

public static class MainMenu
{
    public const string Text =
        "🏠 *Главное меню VKore*\n\nВыбери раздел:";

    public static InlineKeyboardMarkup Keyboard => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("⚙️ Настройки", "menu:settings") },
        new[] { InlineKeyboardButton.WithCallbackData("🔔 Уведомления", "menu:notifications") },
        new[] { InlineKeyboardButton.WithCallbackData("📊 Статус аккаунта VK", "menu:status") },
        new[] { InlineKeyboardButton.WithCallbackData("ℹ️ О боте", "menu:about") },
    });
}

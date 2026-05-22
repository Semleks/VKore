using Telegram.Bot.Types.ReplyMarkups;
using VKore.Core;

namespace VKore.TelegramAPI.Menus;

public static class NotificationsMenu
{
    public static string BuildText(AppConfig cfg) =>
        $"🔔 *Уведомления*\n\n" +
        $"📬 Новые сообщения: {Toggle(cfg.NotifyOnNewMessages)}\n" +
        $"🚨 Удалённые сообщения: {Toggle(cfg.NotifyOnDeletedMessages)}";

    public static InlineKeyboardMarkup BuildKeyboard(AppConfig cfg) => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData(
            $"📬 Новые: {ToggleBtn(cfg.NotifyOnNewMessages)}", "notif:toggle_new") },
        new[] { InlineKeyboardButton.WithCallbackData(
            $"🚨 Удалённые: {ToggleBtn(cfg.NotifyOnDeletedMessages)}", "notif:toggle_deleted") },
        new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", "menu:main") },
    });

    private static string Toggle(bool val) => val ? "✅ Вкл" : "❌ Выкл";
    private static string ToggleBtn(bool val) => val ? "✅" : "❌";
}

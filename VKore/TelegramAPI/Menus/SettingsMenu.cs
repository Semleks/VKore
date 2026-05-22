using Telegram.Bot.Types.ReplyMarkups;
using VKore.Core;

namespace VKore.TelegramAPI.Menus;

public static class SettingsMenu
{
    public static string BuildText(AppConfig cfg) =>
        $"⚙️ *Настройки*\n\n" +
        $"🕐 Задержка: {cfg.MinDelaySeconds}–{cfg.MaxDelaySeconds} сек.\n" +
        $"👍 Авто-лайки: {Toggle(cfg.AutoLikeEnabled)}\n" +
        $"🤖 AI-комментарии: {Toggle(cfg.AiCommentsEnabled)}\n" +
        $"👥 Добавлять друзей: {Toggle(cfg.FriendAutoAddEnabled)} ({cfg.FriendsPerDay} в день)";

    public static InlineKeyboardMarkup BuildKeyboard(AppConfig cfg) => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData(
            $"🕐 Задержка ({cfg.MinDelaySeconds}–{cfg.MaxDelaySeconds}с)", "settings:delay") },
        new[] { InlineKeyboardButton.WithCallbackData(
            $"👍 Авто-лайки: {ToggleBtn(cfg.AutoLikeEnabled)}", "settings:toggle_autolike") },
        new[] { InlineKeyboardButton.WithCallbackData(
            $"🤖 AI-комментарии: {ToggleBtn(cfg.AiCommentsEnabled)}", "settings:toggle_ai") },
        new[] { InlineKeyboardButton.WithCallbackData(
            $"👥 Добавлять друзей: {ToggleBtn(cfg.FriendAutoAddEnabled)}", "settings:toggle_friend_add") },
        new[] { InlineKeyboardButton.WithCallbackData(
            $"📅 Друзей в день: {cfg.FriendsPerDay}", "settings:friends_per_day") },
        new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", "menu:main") },
    });

    private static string Toggle(bool val) => val ? "✅ Вкл" : "❌ Выкл";
    private static string ToggleBtn(bool val) => val ? "✅" : "❌";
}

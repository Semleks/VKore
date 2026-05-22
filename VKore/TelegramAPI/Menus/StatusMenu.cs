using Telegram.Bot.Types.ReplyMarkups;
using VKore.Core;

namespace VKore.TelegramAPI.Menus;

public static class StatusMenu
{
    public static string BuildText(VkUserInfo me) =>
        $"📊 *Статус аккаунта VK*\n\n" +
        $"👤 {me.FirstName} {me.LastName}\n" +
        $"👥 Друзей: {me.FriendsCount}\n" +
        $"🔔 Подписчиков: {me.FollowersCount}";

    public static InlineKeyboardMarkup Keyboard => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", "menu:main") },
    });
}

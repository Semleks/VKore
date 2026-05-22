using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VKore.Core;
using VKore.Infrastructure.Logging;

namespace VKore.VK_Services;

public static class VkSession
{
    public static (VkApi api, VkUserInfo me) Initialize(string token)
    {
        var api = new VkApi();
        api.Authorize(new ApiAuthParams { AccessToken = token });

        var profile = api.Users
            .Get(Array.Empty<long>(), ProfileFields.All)
            .FirstOrDefault()
            ?? throw new Exception("Не удалось получить профиль VK. Проверьте токен.");

        DateTime? birthDate = null;
        if (!string.IsNullOrWhiteSpace(profile.BirthDate))
        {
            var parts = profile.BirthDate.Split('.');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int d) &&
                int.TryParse(parts[1], out int m) &&
                int.TryParse(parts[2], out int y))
            {
                birthDate = new DateTime(y, m, d);
            }
        }

        var me = new VkUserInfo
        {
            Id             = profile.Id,
            FirstName      = profile.FirstName     ?? string.Empty,
            LastName       = profile.LastName      ?? string.Empty,
            FriendsCount   = profile.Counters?.Friends ?? 0,
            FollowersCount = profile.FollowersCount ?? 0,
            BirthDate      = birthDate,
        };

        AppLogger.Log($"Авторизован: {me.FirstName} {me.LastName}", LogType.System);
        return (api, me);
    }
}
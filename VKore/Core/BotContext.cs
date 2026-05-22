using VkNet;

namespace VKore.Core;

public class VkUserInfo
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int FriendsCount { get; set; }
    public long FollowersCount { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class BotContext
{
    public VkApi VkApi { get; set; } = null!;
    public AppConfig Config { get; set; } = null!;
    public VkUserInfo Me { get; set; } = null!;
}

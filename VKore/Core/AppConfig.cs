namespace VKore.Core;

public class AppConfig
{
    public string TelegramToken { get; set; } = string.Empty;
    public string TelegramUserId { get; set; } = string.Empty;

    public int MinDelaySeconds { get; set; } = 4;
    public int MaxDelaySeconds { get; set; } = 8;
    public bool AutoLikeEnabled { get; set; } = false;
    public bool AiCommentsEnabled { get; set; } = false;
    public bool FriendAutoAddEnabled { get; set; } = false;
    public int FriendsPerDay { get; set; } = 3;

    public bool NotifyOnNewMessages { get; set; } = true;
    public bool NotifyOnDeletedMessages { get; set; } = true;
}

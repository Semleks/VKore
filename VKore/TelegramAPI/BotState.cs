namespace VKore.TelegramAPI;

public enum MenuScreen
{
    None,
    Main,
    Settings,
    Notifications,
    Status,
    About,
    AwaitingMinDelay,
    AwaitingMaxDelay,
    AwaitingFriendsPerDay,
}

public static class BotState
{
    private static readonly Dictionary<long, MenuScreen> States = new();

    public static MenuScreen Get(long userId) =>
        States.TryGetValue(userId, out var s) ? s : MenuScreen.None;

    public static void Set(long userId, MenuScreen screen) =>
        States[userId] = screen;
}

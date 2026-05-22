namespace VKore.Infrastructure.Logging;

public enum LogType
{
    System,
    Debug,
    Warning,
    Error
}

public static class AppLogger
{
    // чтоб цвета не перебивали друг друга при параллельных задачах
    private static readonly object _lock = new();

    public static void Log(string message, LogType type = LogType.System)
    {
        ConsoleColor color = type switch
        {
            LogType.Error   => ConsoleColor.Red,
            LogType.Warning => ConsoleColor.Yellow,
            LogType.Debug   => ConsoleColor.DarkGray,
            _               => ConsoleColor.Cyan
        };

        lock (_lock)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{type}] {DateTime.Now:HH:mm:ss}: {message}");
            Console.ForegroundColor = prev;
        }
    }
}

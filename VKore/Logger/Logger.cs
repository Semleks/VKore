namespace VKore.Logger;

public enum LogType
{
    System,
    Debug,
    Warning,
    Error
}

public class Logger
{
    public static void Log(string message, LogType type)
    {
        var originalColor = Console.ForegroundColor;
        
        Console.ForegroundColor = type switch
        {
            LogType.System => ConsoleColor.Cyan,
            LogType.Debug => ConsoleColor.DarkGray,
            LogType.Warning => ConsoleColor.Yellow,
            LogType.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        var time = DateTime.Now.ToString("HH:mm:ss");
        
        Console.WriteLine($"[{type}] {time}: {message}");

        Console.ForegroundColor = originalColor;
    }
}
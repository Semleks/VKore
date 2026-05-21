namespace VKore.Logger;

public class Logger
{
    
    
    public static void Log(string message, int status)
    {
        var statusString = "";

        switch (status)
        {
            case 0:
                statusString = "System";
                break;
            case 1:
                statusString = "Debug";
                break;
            case 2:
                statusString = "Warning";
                break;
            case 3:
                statusString = "Error";
                break;
        }

        Console.WriteLine($"[{statusString}] {DateTime.Now}: {message}");
    }
}
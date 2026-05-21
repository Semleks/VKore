namespace VKore.System;

public class BotHelper
{
    private static readonly Random Random = new();
    
    public static async Task DelayRandomlyAsync(int minSeconds = 3, int maxSeconds = 8)
    {
        var delayMs = Random.Next(minSeconds * 1000, maxSeconds * 1000);
        await Task.Delay(delayMs);
    }
}
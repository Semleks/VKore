using VKore.Core;

namespace VKore.VK_Services;

public static class HumanDelay
{
    private static readonly Random _rnd = new();

    public static Task WaitAsync(AppConfig cfg) =>
        Task.Delay(_rnd.Next(cfg.MinDelaySeconds * 1000, cfg.MaxDelaySeconds * 1000));
}

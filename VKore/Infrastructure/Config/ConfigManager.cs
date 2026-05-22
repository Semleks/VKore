using System.Text.Json;
using VKore.Core;
using VKore.Infrastructure.Logging;

namespace VKore.Infrastructure.Config;

public static class ConfigManager
{
    private static readonly string _path = Path.Combine(AppContext.BaseDirectory, "config.json");

    public static async Task<AppConfig> LoadOrCreateAsync()
    {
        if (!File.Exists(_path))
            return new AppConfig();

        try
        {
            var json = await File.ReadAllTextAsync(_path);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            AppLogger.Log("config.json повреждён, создаю чистый.", LogType.Warning);
            return new AppConfig();
        }
    }

    public static async Task SaveAsync(AppConfig config)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(config, opts));
    }

    public static bool IsValid(AppConfig config) =>
        !string.IsNullOrWhiteSpace(config.TelegramToken) &&
        !string.IsNullOrWhiteSpace(config.TelegramUserId);
}
using dotenv.net;
using dotenv.net.Utilities;
using VKore.Infrastructure.Config;
using VKore.Infrastructure.Logging;
using VKore.TelegramAPI;
using VKore.VK_Services;
using VKore.Core;

DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

Console.WriteLine(
    "\n██╗░░░██╗██╗░░██╗░█████╗░██████╗░███████╗\n" +
    "██║░░░██║██║░██╔╝██╔══██╗██╔══██╗██╔════╝\n" +
    "╚██╗░██╔╝█████═╝░██║░░██║██████╔╝█████╗░░\n" +
    "░╚████╔╝░██╔═██╗░██║░░██║██╔══██╗██╔══╝░░\n" +
    "░░╚██╔╝░░██║░╚██╗╚█████╔╝██║░░██║███████╗\n" +
    "░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░╚═╝░░╚═╝╚══════╝\n");
Console.WriteLine("Open-Source автоматизация ВКонтакте  •  github.com/Semleks/VKore\n");

var vkToken = EnvReader.GetStringValue("VK_API_KEY");
if (string.IsNullOrWhiteSpace(vkToken))
{
    AppLogger.Log("VK_API_KEY не найден — заполни .env и перезапусти.", LogType.Error);
    return;
}

var config = await ConfigManager.LoadOrCreateAsync();

if (!ConfigManager.IsValid(config))
{
    config = await RunFirstSetupAsync(config);
    await ConfigManager.SaveAsync(config);
}

var (vkApi, me) = VkSession.Initialize(vkToken);

var ctx = new BotContext
{
    VkApi  = vkApi,
    Config = config,
    Me     = me,
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var bot = new BotRunner(ctx);
bot.Start(cts.Token);

AppLogger.Log($"Привет, {me.FirstName}! Всё запущено.");

if (config.AutoLikeEnabled)
    AppLogger.Log("Авто-лайки включены.");

var monitor     = new MessageMonitor(ctx, bot.Client);
var friendAdder = new FriendAdder(ctx, bot.Client);

await Task.WhenAll(
    monitor.RunAsync(cts.Token),
    friendAdder.RunAsync(cts.Token)
);

AppLogger.Log("Выключаюсь.");
return;

static Task<AppConfig> RunFirstSetupAsync(AppConfig existing)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Ух, кто впервые зашёл в VKore? Давай всё настроим :)");
    Console.ResetColor();
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠️  Если ты из России — без VPN Telegram Bot API не работает.");
    Console.WriteLine("   Включи VPN прямо сейчас, до того как вводить токен.");
    Console.ResetColor();
    Console.WriteLine();

    string token;
    while (true)
    {
        Console.Write("Токен Telegram-бота (получи у @BotFather): ");
        token = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!string.IsNullOrEmpty(token)) break;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Токен не может быть пустым, попробуй ещё раз.");
        Console.ResetColor();
    }

    string userId;
    while (true)
    {
        Console.Write("Твой Telegram ID (узнай у @userinfobot): ");
        userId = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!string.IsNullOrEmpty(userId) && long.TryParse(userId, out _)) break;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ID должен быть числом, попробуй ещё раз.");
        Console.ResetColor();
    }

    existing.TelegramToken  = token;
    existing.TelegramUserId = userId;

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\nГотово! Настройки сохранены. Запускаю...\n");
    Console.ResetColor();

    return Task.FromResult(existing);
}
using VKore.Logger;

namespace VKore.Options;

public class AccountOptions 
{ 
    // VK User
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
    public string Status { get; set; } 
    public DateTime? LastOnline { get; set; }
    public long SubscribersCount { get; set; } 
    public int FriendsCount { get; set; }
    
    //Telegram Settings
    public static bool TelegramForwardingEnabled { get; set; } = false;
    public static string TelegramBotToken { get; set; } = "";
    public static string TelegramChatId { get; set; } = "";
    
    
    // Settings of the Program
    public static int MinDelaySeconds { get; set; } = 3;
    public static int MaxDelaySeconds { get; set; } = 8;
    public static bool AutoLikeEnabled { get; set; } = true;
    public static bool EnableAiComments { get; set; } = false;

    public static void ShowSettingsMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=================================");
            Console.WriteLine("       НАСТРОЙКИ VKore           ");
            Console.WriteLine("=================================");
            Console.WriteLine($"1. Мин. задержка: {MinDelaySeconds} сек.");
            Console.WriteLine($"2. Макс. задержка: {MaxDelaySeconds} сек.");
            Console.WriteLine($"3. Авто-лайки друзьям: {(AutoLikeEnabled ? "ВКЛ" : "ВЫКЛ")}");
            Console.WriteLine($"4. ИИ-комментарии: {(EnableAiComments ? "ВКЛ" : "ВЫКЛ")}");
            Console.WriteLine($"5. Пересылка в Telegram: {(TelegramForwardingEnabled ? "ВКЛ" : "ВЫКЛ")}");
            Console.WriteLine($"6. Токен TG Бота: {(string.IsNullOrEmpty(TelegramBotToken) ? "Не настроен" : "Настроен")}");
            Console.WriteLine($"7. Chat ID получателя: {(string.IsNullOrEmpty(TelegramChatId) ? "Не настроен" : TelegramChatId)}");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("0. Вернуться в главное меню");
            Console.WriteLine("=================================");
            Console.Write("\nВыберите пункт (цифру): ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Logger.Logger.Log("Ошибка: введите число!", LogType.Error);
                Console.ReadKey();
                continue;
            }

            switch (choice)
            {
                case 1:
                    Console.Write("Введите мин. задержку (сек): ");
                    if (int.TryParse(Console.ReadLine(), out int min) && min > 0)
                    {
                        MinDelaySeconds = min;
                        Logger.Logger.Log("Минимальная задержка изменена.", LogType.System);
                    }
                    else 
                        Logger.Logger.Log("Неверное значение!", LogType.Warning);
                    break;

                case 2:
                    Console.Write("Введите макс. задержку (сек): ");
                    if (int.TryParse(Console.ReadLine(), out int max) && max >= MinDelaySeconds)
                    {
                        MaxDelaySeconds = max;
                        Logger.Logger.Log("Максимальная задержка изменена.", LogType.System);
                    }
                    else 
                        Logger.Logger.Log("Неверно! Значение должно быть больше минимального.", LogType.Warning);
                    break;

                case 3:
                    AutoLikeEnabled = !AutoLikeEnabled;
                    Logger.Logger.Log($"Авто-лайки теперь: {(AutoLikeEnabled ? "ВКЛ" : "ВЫКЛ")}", LogType.System);
                    break;

                case 4:
                    EnableAiComments = !EnableAiComments;
                    Logger.Logger.Log($"ИИ-комментарии теперь: {(EnableAiComments ? "ВКЛ" : "ВЫКЛ")}", LogType.System);
                    break;
                
                case 5:
                    if (string.IsNullOrEmpty(TelegramBotToken) || string.IsNullOrEmpty(TelegramChatId))
                    {
                        Logger.Logger.Log("Ошибка: Сначала настройте Токен Бота (6) и Chat ID (7)!", LogType.Error);
                    }
                    else
                    {
                        TelegramForwardingEnabled = !TelegramForwardingEnabled;
                        Logger.Logger.Log($"Пересылка в Telegram теперь: {(TelegramForwardingEnabled ? "ВКЛ" : "ВЫКЛ")}", LogType.System);
                    }
                    break;

                case 6:
                    Console.Write("Введите токен Telegram-бота (из @BotFather): ");
                    string inputToken = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(inputToken))
                    {
                        TelegramBotToken = inputToken;
                        Logger.Logger.Log("Токен Telegram-бота успешно сохранен.", LogType.System);
                    }
                    else
                    {
                        Logger.Logger.Log("Ошибка: Токен не может быть пустым!", LogType.Warning);
                    }
                    
                    break;

                case 7:
                    Console.Write("Введите ваш Telegram Chat ID: ");
                    string inputChatId = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(inputChatId))
                    {
                        TelegramChatId = inputChatId;
                        Logger.Logger.Log("Telegram Chat ID успешно сохранен.", LogType.System);
                    }
                    else
                    {
                        Logger.Logger.Log("Ошибка: Chat ID не может быть пустым!", LogType.Warning);
                    }
                    break;

                case 0:
                    return; // Выход из метода (возврат в главное меню)

                default:
                    Logger.Logger.Log("Неверный пункт меню!", LogType.Warning);
                    Console.ReadKey();
                    break;
            }
        }
    }
}
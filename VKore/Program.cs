using System;
using System.Threading.Tasks;
using dotenv.net;
using dotenv.net.Utilities;
using VKore.Logger;
using VKore.Options;
using VKore.System;
using VKore.VK_Services;

class Program
{
    private static async Task Main()
    {
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

        var apiKey = EnvReader.GetStringValue("VK_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.Log("!!! ОШИБКА: Ключ API VK не найден. Пожалуйста, проверьте файл .env и окружение.", LogType.Error);
            return; 
        }

        var vkSession = new VkSession();
        vkSession.Initialize(apiKey);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==============================");
            Console.WriteLine("\n██╗░░░██╗██╗░░██╗░█████╗░██████╗░███████╗\n██║░░░██║██║░██╔╝██╔══██╗██╔══██╗██╔════╝\n╚██╗░██╔╝█████═╝░██║░░██║██████╔╝█████╗░░\n░╚████╔╝░██╔═██╗░██║░░██║██╔══██╗██╔══╝░░\n░░╚██╔╝░░██║░╚██╗╚█████╔╝██║░░██║███████╗\n░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░╚═╝░░╚═╝╚══════╝");
            Console.WriteLine("==============================\n\n");

            Console.WriteLine("Open-Source проект автоматизации рутинных задач во VKontakte");
            Console.WriteLine("Main developer: Semlex");
            Console.WriteLine("Link to GitHub: https://github.com/Semleks/VKore");
            Console.WriteLine("\n\n");
            Console.WriteLine("Выберите нужный пункт (напишите цифру):");
            Console.WriteLine("1. Настройки");
            Console.WriteLine("2. Настройки сервиса (демона, Linux only)");
            Console.WriteLine("3. Статистика");
            Console.WriteLine("4. Настройка ИИ");
            Console.WriteLine("5. Запустить одноразово");
            Console.WriteLine("0. Выход");
            Console.Write("\nВаш выбор: ");

            if (!int.TryParse(Console.ReadLine(), out int choose))
            {
                Logger.Log("Неверный ввод! Пожалуйста, введите цифру.", LogType.Warning);
                await Task.Delay(1500);
                continue;
            }

            switch (choose)
            {
                case 1:
                    AccountOptions.ShowSettingsMenu();
                    break;

                case 2:
                case 3:
                case 4:
                    Logger.Log("Этот раздел находится в разработке. Скоро добавим!", LogType.System);
                    Console.WriteLine("\nНажмите любую клавишу для возврата...");
                    Console.ReadKey();
                    break;

                case 5:
                    Console.Clear();
                    
                    Logger.Log("=== Запуск сессии ===", LogType.System);
                    Logger.Log($"Привет, {vkSession.Me.FirstName} {vkSession.Me.LastName}!", LogType.Warning);
                    
                    if (AccountOptions.AutoLikeEnabled)
                    {
                        Logger.Log("Автоматические лайки включены.", LogType.System);
                    }
                    else
                    {
                        Logger.Log("Авто-лайки отключены пользователем в настройках.", LogType.Warning);
                    }

                    if (AccountOptions.EnableAiComments)
                    {
                        Logger.Log("ИИ-комментарии включены.", LogType.Debug);
                    }

                    await MessageMonitor.StartAsync(vkSession.Api);
                    Console.ReadKey();
                    break;

                case 0:
                    Logger.Log("Выход из программы. До встречи!", LogType.System);
                    return;

                default:
                    Logger.Log("Такого пункта нет в меню!", LogType.Warning);
                    await Task.Delay(1500);
                    break;
            }
        }
    }
}
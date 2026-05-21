using dotenv.net;
using dotenv.net.Utilities;
using VKore.Logger;
using VKore.VK_Services;

class Program
{
    private static void Main()
    {
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        
        Console.WriteLine("==============================");
        Console.WriteLine("\n██╗░░░██╗██╗░░██╗░█████╗░██████╗░███████╗\n██║░░░██║██║░██╔╝██╔══██╗██╔══██╗██╔════╝\n╚██╗░██╔╝█████═╝░██║░░██║██████╔╝█████╗░░\n░╚████╔╝░██╔═██╗░██║░░██║██╔══██╗██╔══╝░░\n░░╚██╔╝░░██║░╚██╗╚█████╔╝██║░░██║███████╗\n░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░╚═╝░░╚═╝╚══════╝");
        Console.WriteLine("==============================\n\n");

        Console.WriteLine("Open-Source проект автоматизации рутинных задач во VKontakte");
        Console.WriteLine("Main developer: Semlex");
        Console.WriteLine("Link to GitHub: ");
        Console.WriteLine("\n\n");
        Console.WriteLine("Выберите нужный пункт (напишите цифру):\n1. Настройки\n2. Настройки сервиса (демона, Linux only)\n3. Статистика\n4. Настройка ИИ\n5. Запустить одноразово");
        
        string apiKey = EnvReader.GetStringValue("VK_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.Log("!!! ОШИБКА: Ключ API VK не найден. Пожалуйста, проверьте файл .env и окружение.", 3);
            return; 
        }
        
        Console.WriteLine("\n\n");
        var vkSession = new VkSession();
        vkSession.Initialize(apiKey);
        
        while (true)
        {
            var choose = int.Parse(Console.ReadLine());

            switch (choose)
            {
                case 1:
                    
                    return;
            }
            
        }
    }
}
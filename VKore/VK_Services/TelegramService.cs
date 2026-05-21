using VKore.Logger;

namespace VKore.VK_Services;

public class TelegramService
{
    private static readonly HttpClient HttpClient = new();

    public static async Task SendMessageAsync(string botToken, string chatId, string text)
    {
        if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(chatId))
            return;

        try
        {
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(chatId), "chat_id");
            content.Add(new StringContent(text), "text");
            content.Add(new StringContent("Markdown"), "parse_mode");

            var response = await HttpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Logger.Logger.Log($"Ошибка отправки в TG: {response.StatusCode} - {errorMsg}", LogType.Warning);
                Logger.Logger.Log("Если вы из РФ, может, стоит подключить VPN?", LogType.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log($"Ошибка при отправке сообщения в TG: {ex.Message}", LogType.Error);
            Logger.Logger.Log("Если вы из РФ, может, стоит подключить VPN?", LogType.Error);
        }
    }
}
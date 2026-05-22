using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VKore.Core;
using VKore.Infrastructure.Logging;
using VKore.TelegramAPI.Handlers;

namespace VKore.TelegramAPI;

public class BotRunner
{
    private readonly TelegramBotClient _client;
    private readonly MessageHandler _messages;
    private readonly CallbackHandler _callbacks;
    private readonly long _ownerId;

    public BotRunner(BotContext ctx)
    {
        _client    = new TelegramBotClient(ctx.Config.TelegramToken);
        _ownerId   = long.Parse(ctx.Config.TelegramUserId);
        _messages  = new MessageHandler(_client, ctx);
        _callbacks = new CallbackHandler(_client, ctx);
    }

    public ITelegramBotClient Client => _client;

    public void Start(CancellationToken ct = default)
    {
        var opts = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
        _client.StartReceiving(OnUpdate, OnError, opts, ct);
        AppLogger.Log("[Telegram] Бот запущен, жду команды.");
    }

    private async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message is { } msg)
            {
                if (!IsOwner(msg.From?.Id))
                {
                    await bot.SendMessage(msg.Chat.Id, "Этот бот приватный.", cancellationToken: ct);
                    return;
                }
                await _messages.HandleAsync(msg, ct);
            }
            else if (update.CallbackQuery is { } cb)
            {
                if (!IsOwner(cb.From.Id)) return;
                await _callbacks.HandleAsync(cb, ct);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"[Telegram] Ошибка: {ex.Message}", LogType.Error);
        }
    }

    private Task OnError(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        var msg = ex is ApiRequestException api
            ? $"TG API [{api.ErrorCode}]: {api.Message}"
            : ex.ToString();

        AppLogger.Log($"[Telegram] {msg}", LogType.Error);
        return Task.CompletedTask;
    }

    private bool IsOwner(long? id) => id == _ownerId;
}

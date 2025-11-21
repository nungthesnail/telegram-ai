using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Infrastructure.Options;

namespace TelegramAi.TelegramBot;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TelegramOptions> _options;
    private ITelegramBotClient? _botClient;

    public TelegramBotHostedService(
        ILogger<TelegramBotHostedService> logger,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TelegramOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = _options.CurrentValue.BotToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Telegram bot token is missing. Hosted service will be skipped.");
            return;
        }

        _botClient = new TelegramBotClient(token);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Enum.GetValues<UpdateType>()
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Telegram bot started polling");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } text })
        {
            return;
        }

        var chatId = new ChatId(update.Message.Chat.Id);

        if (text.StartsWith("/verify", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                await botClient.SendMessage(chatId,
                    "Используйте /verify <channelId> <code>", cancellationToken: cancellationToken);
                return;
            }

            if (!Guid.TryParse(parts[1], out var channelId))
            {
                await botClient.SendMessage(chatId,
                    "Неверный формат channelId", cancellationToken: cancellationToken);
                return;
            }

            var verificationCode = parts[2];

            using var scope = _scopeFactory.CreateScope();
            var channelService = scope.ServiceProvider.GetRequiredService<IChannelService>();

            try
            {
                var botId = botClient.BotId;

                await channelService.ConfirmBotLinkAsync(
                    new ConfirmBotLinkRequest(channelId, verificationCode, update.Message.Chat.Id, botId),
                    cancellationToken);

                await botClient.SendMessage(chatId,
                    "Канал успешно подтвержден.", cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to confirm channel via Telegram");
                await botClient.SendMessage(chatId,
                    "Не удалось подтвердить канал. Проверьте код.", cancellationToken: cancellationToken);
            }
        }
    }
}



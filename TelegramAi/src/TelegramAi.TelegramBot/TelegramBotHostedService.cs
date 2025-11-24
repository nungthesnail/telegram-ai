using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Options;
using TelegramAi.Infrastructure.Persistence;

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
        // Обработка добавления бота в канал
        if (update.MyChatMember is { Chat: { Type: ChatType.Channel } chat, NewChatMember.Status: ChatMemberStatus.Member or ChatMemberStatus.Administrator })
        {
            var userId = update.MyChatMember.From.Id;
            await HandleBotAddedToChannelAsync(
                botClient, chat.Id, botClient.BotId, userId, cancellationToken);
            return;
        }

        // Обработка команд от пользователей
        if (update.Message is not { Text: { } text, From: { } from })
        {
            return;
        }

        var chatId = new ChatId(update.Message.Chat.Id);
        var telegramUserId = from.Id;

        if (text.StartsWith("/verify", StringComparison.OrdinalIgnoreCase))
        {
            await HandleVerifyCommandAsync(botClient, chatId, telegramUserId, text, cancellationToken);
        }
    }

    private async Task HandleBotAddedToChannelAsync(ITelegramBotClient botClient, long chatId, long botId, long telegramUserId,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var channelService = scope.ServiceProvider.GetRequiredService<IChannelService>();

        try
        {
            var channel = await channelService.LinkChannelFromTelegramAsync(chatId, botId, telegramUserId, cancellationToken);
            await botClient.SendMessage(telegramUserId, $"Бот успешно добавлен в канал {channel.Title}",
                cancellationToken: cancellationToken);
            _logger.LogInformation("Bot added to channel {ChatId}, channel {ChannelId} created (pending user confirmation)", chatId, channel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link channel from Telegram chat {ChatId}", chatId);
        }
    }

    private async Task HandleVerifyCommandAsync(ITelegramBotClient botClient, ChatId chatId, long telegramUserId, string text, CancellationToken cancellationToken)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await botClient.SendMessage(chatId,
                "Используйте /verify <code>\n\nПолучите код в веб-интерфейсе и отправьте его здесь для подтверждения вашего аккаунта.",
                cancellationToken: cancellationToken);
            return;
        }

        var verificationCode = parts[1];

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        try
        {
            // Ищем пользователя по коду верификации
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var codeEntity = await dbContext.UserVerificationCodes
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.VerificationCode == verificationCode &&
                    x.VerifiedAtUtc == null &&
                    x.ExpiresAtUtc > DateTimeOffset.UtcNow, cancellationToken);

            if (codeEntity == null)
            {
                await botClient.SendMessage(chatId,
                    "Код подтверждения не найден или истек. Получите новый код в веб-интерфейсе.",
                    cancellationToken: cancellationToken);
                return;
            }

            await userService.ConfirmTelegramUserAsync(
                codeEntity.UserId,
                verificationCode,
                telegramUserId,
                cancellationToken);

            await botClient.SendMessage(chatId,
                "✅ Ваш аккаунт успешно подтвержден! Теперь вы можете использовать все функции бота.",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to confirm user via Telegram");
            var errorMessage = ex.Message.Contains("already linked")
                ? "Этот Telegram аккаунт уже привязан к другому пользователю."
                : "Не удалось подтвердить аккаунт. Проверьте код или получите новый в веб-интерфейсе.";
            await botClient.SendMessage(chatId, errorMessage, cancellationToken: cancellationToken);
        }
    }
}



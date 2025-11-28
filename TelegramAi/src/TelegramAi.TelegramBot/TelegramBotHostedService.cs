using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.TelegramBot;

public class TelegramBotHostedService(
    ILogger<TelegramBotHostedService> logger,
    IServiceScopeFactory scopeFactory,
    TelegramBotClient botClient)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Enum.GetValues<UpdateType>()
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        logger.LogInformation("Telegram bot started polling");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        // Обработка PreCheckout Query
        if (update.PreCheckoutQuery is { } preCheckoutQuery)
        {
            await bot.AnswerPreCheckoutQuery(preCheckoutQuery.Id, null, cancellationToken);
            return;
        }
        
        // Обработка SuccessfulPayment
        if (update.Message?.SuccessfulPayment is { } successfulPayment)
        {
            await HandleSuccessfulPaymentAsync(bot, update.Message.From!.Id, successfulPayment, cancellationToken);
            return;
        }

        // Обработка добавления бота в канал
        if (update.MyChatMember is { Chat: { Type: ChatType.Channel } chat, NewChatMember.Status: ChatMemberStatus.Member or ChatMemberStatus.Administrator })
        {
            var userId = update.MyChatMember.From.Id;
            await HandleBotAddedToChannelAsync(
                bot, chat.Id, bot.BotId, userId, cancellationToken);
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
            await HandleVerifyCommandAsync(bot, chatId, telegramUserId, text, cancellationToken);
        }
    }

    private async Task HandleBotAddedToChannelAsync(ITelegramBotClient bot, long chatId, long botId, long telegramUserId,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var channelService = scope.ServiceProvider.GetRequiredService<IChannelService>();

        try
        {
            var channel = await channelService.LinkChannelFromTelegramAsync(chatId, botId, telegramUserId, cancellationToken);
            await bot.SendMessage(telegramUserId, $"Бот успешно добавлен в канал {channel.Title}",
                cancellationToken: cancellationToken);
            logger.LogInformation("Bot added to channel {ChatId}, channel {ChannelId} created (pending user confirmation)", chatId, channel.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to link channel from Telegram chat {ChatId}", chatId);
        }
    }

    private async Task HandleVerifyCommandAsync(ITelegramBotClient bot, ChatId chatId, long telegramUserId, string text, CancellationToken cancellationToken)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await bot.SendMessage(chatId,
                "Используйте /verify <code>\n\nПолучите код в веб-интерфейсе и отправьте его здесь для подтверждения вашего аккаунта.",
                cancellationToken: cancellationToken);
            return;
        }

        var verificationCode = parts[1];

        await using var scope = scopeFactory.CreateAsyncScope();
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
                await bot.SendMessage(chatId,
                    "Код подтверждения не найден или истек. Получите новый код в веб-интерфейсе.",
                    cancellationToken: cancellationToken);
                return;
            }

            await userService.ConfirmTelegramUserAsync(
                codeEntity.UserId,
                verificationCode,
                telegramUserId,
                cancellationToken);

            await bot.SendMessage(chatId,
                "✅ Ваш аккаунт успешно подтвержден! Теперь вы можете использовать все функции бота.",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to confirm user via Telegram");
            var errorMessage = ex.Message.Contains("already linked")
                ? "Этот Telegram аккаунт уже привязан к другому пользователю."
                : "Не удалось подтвердить аккаунт. Проверьте код или получите новый в веб-интерфейсе.";
            await bot.SendMessage(chatId, errorMessage, cancellationToken: cancellationToken);
        }
    }

    private async Task HandleSuccessfulPaymentAsync(ITelegramBotClient bot, long telegramUserId, Telegram.Bot.Types.Payments.SuccessfulPayment successfulPayment, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            
            // Извлекаем planId из payload
            if (!Guid.TryParse(successfulPayment.InvoicePayload, out var planId))
            {
                logger.LogWarning("Invalid plan ID in payment payload: {Payload}", successfulPayment.InvoicePayload);
                await bot.SendMessage(telegramUserId,
                    "❌ Ошибка обработки платежа: неверный идентификатор плана.",
                    cancellationToken: cancellationToken);
                return;
            }

            try
            {
                await subscriptionService.ProcessTelegramPaymentAsync(
                    telegramUserId,
                    successfulPayment.TelegramPaymentChargeId,
                    (decimal)successfulPayment.TotalAmount / 100, // Telegram возвращает сумму в копейках
                    successfulPayment.Currency,
                    planId,
                    cancellationToken);

                await bot.SendMessage(telegramUserId,
                    "✅ Платеж успешно обработан! Ваша подписка активирована.",
                    cancellationToken: cancellationToken);
                logger.LogInformation("Payment processed successfully for user {TelegramUserId}, plan {PlanId}",
                    telegramUserId, planId);
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Failed to process payment id={id}", successfulPayment.TelegramPaymentChargeId);
                await bot.SendMessage(telegramUserId,
                    "❌ Ошибка обработки платежа. Пожалуйста, обратитесь в поддержку.",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception exc)
        {
            logger.LogCritical(exc, "Error processing successful payment for user {TelegramUserId}", telegramUserId);
            await bot.SendMessage(telegramUserId,
                "❌ Произошла ошибка при обработке платежа. Пожалуйста, обратитесь в поддержку.",
                cancellationToken: cancellationToken);
        }
    }
}

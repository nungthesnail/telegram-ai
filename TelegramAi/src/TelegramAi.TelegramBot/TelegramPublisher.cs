using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Payments;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Entities;

namespace TelegramAi.TelegramBot;

public class TelegramPublisher(ILogger<TelegramPublisher> logger, TelegramBotClient bot) : ITelegramPublisher
{
    public Task<int> PublishAsync(Guid channelId, string text, CancellationToken cancellationToken)
    {
        var messageId = DateTimeOffset.UtcNow.GetHashCode();
        logger.LogInformation("Simulated publishing post to channel {ChannelId}: {Text}", channelId, text);
        return Task.FromResult(messageId);
    }

    public Task UpdateProfileAsync(Guid channelId, string title, string description, CancellationToken cancellationToken)
    {
        logger.LogInformation("Simulated channel profile update for {ChannelId}. Title: {Title}", channelId, title);
        return Task.CompletedTask;
    }

    public async Task SendInvoiceAsync(long userId, SubscriptionPlan plan, CancellationToken cancellationToken)
    {
        await bot.SendInvoice(
            chatId: userId,
            title: plan.Name,
            description: plan.Description,
            payload: plan.Id.ToString(),
            providerToken: null,
            currency: "RUB",
            prices:
            [
                new LabeledPrice(label: plan.Name, amount: (int)(plan.PriceRub * 100))
            ],
            cancellationToken: cancellationToken);
    }
}

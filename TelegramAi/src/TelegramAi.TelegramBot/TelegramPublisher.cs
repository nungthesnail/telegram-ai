using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Entities;

namespace TelegramAi.TelegramBot;

public class TelegramPublisher(ILogger<TelegramPublisher> logger, TelegramBotClient bot) : ITelegramPublisher
{
    public async Task<int> PublishAsync(long chatId, ChannelPostDto post, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Publishing post id={postId} to chat id={chatId}", post.Id, chatId);
            return (await bot.SendMessage(
                chatId: chatId,
                text: post.Content,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken)).MessageId;
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error while publishing post id={postId} to chat id={chatId}", post.Id, chatId);
            throw;
        }
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

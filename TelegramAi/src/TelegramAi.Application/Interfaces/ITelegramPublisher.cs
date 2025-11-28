using TelegramAi.Domain.Entities;

namespace TelegramAi.Application.Interfaces;

public interface ITelegramPublisher
{
    Task<int> PublishAsync(Guid channelId, string text, CancellationToken cancellationToken);
    Task UpdateProfileAsync(Guid channelId, string title, string description, CancellationToken cancellationToken);
    Task SendInvoiceAsync(long userId, SubscriptionPlan plan, CancellationToken cancellationToken);
}

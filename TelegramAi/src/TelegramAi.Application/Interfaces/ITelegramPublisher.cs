namespace TelegramAi.Application.Interfaces;

public interface ITelegramPublisher
{
    Task<string> PublishAsync(Guid channelId, string text, CancellationToken cancellationToken);
    Task UpdateProfileAsync(Guid channelId, string title, string description, CancellationToken cancellationToken);
}



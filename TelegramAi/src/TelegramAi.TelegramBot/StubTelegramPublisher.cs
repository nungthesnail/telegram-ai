using Microsoft.Extensions.Logging;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.TelegramBot;

public class StubTelegramPublisher : ITelegramPublisher
{
    private readonly ILogger<StubTelegramPublisher> _logger;

    public StubTelegramPublisher(ILogger<StubTelegramPublisher> logger)
    {
        _logger = logger;
    }

    public Task<int> PublishAsync(Guid channelId, string text, CancellationToken cancellationToken)
    {
        var messageId = DateTimeOffset.UtcNow.GetHashCode();
        _logger.LogInformation("Simulated publishing post to channel {ChannelId}: {Text}", channelId, text);
        return Task.FromResult(messageId);
    }

    public Task UpdateProfileAsync(Guid channelId, string title, string description, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulated channel profile update for {ChannelId}. Title: {Title}", channelId, title);
        return Task.CompletedTask;
    }
}



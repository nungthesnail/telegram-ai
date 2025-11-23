namespace TelegramAi.Application.Interfaces;

public interface ITelegramChannelInfoProvider
{
    Task<TelegramChannelInfo> GetChannelInfoAsync(long chatId, CancellationToken cancellationToken);
}

public record TelegramChannelInfo(
    string Title,
    string? Description,
    string? Username,
    string? Link);


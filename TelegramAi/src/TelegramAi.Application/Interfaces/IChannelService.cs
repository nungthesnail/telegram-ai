using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IChannelService
{
    Task<ChannelDto> CreateAsync(Guid userId, CreateChannelRequest request, CancellationToken cancellationToken);
    Task<ChannelDto> UpdateAsync(Guid userId, Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken);
    Task<ChannelDto> UpdateAiDescriptionAsync(Guid userId, Guid channelId, string aiDescription, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChannelDto>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<ChannelDto?> GetAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
    Task<ChannelDto> LinkChannelFromTelegramAsync(long telegramChatId, long telegramBotId, long? telegramUserId,
        CancellationToken cancellationToken);
    Task<ChannelDto> ConfirmChannelLinkAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
}



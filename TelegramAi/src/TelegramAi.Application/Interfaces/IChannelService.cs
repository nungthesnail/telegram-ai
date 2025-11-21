using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IChannelService
{
    Task<ChannelDto> CreateAsync(Guid userId, CreateChannelRequest request, CancellationToken cancellationToken);
    Task<ChannelDto> UpdateAsync(Guid userId, Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChannelDto>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<ChannelDto?> GetAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
    Task<ChannelBotLinkDto> RequestBotLinkAsync(Guid userId, RequestBotLinkRequest request, CancellationToken cancellationToken);
    Task<ChannelBotLinkDto> ConfirmBotLinkAsync(ConfirmBotLinkRequest request, CancellationToken cancellationToken);
}



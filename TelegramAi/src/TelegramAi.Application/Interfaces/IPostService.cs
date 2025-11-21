using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IPostService
{
    Task<ChannelPostDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken);
    Task<ChannelPostDto> UpdateAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken cancellationToken);
    Task<ChannelPostDto> ScheduleAsync(Guid userId, Guid postId, SchedulePostRequest request, CancellationToken cancellationToken);
    Task<ChannelPostDto> PublishAsync(Guid userId, Guid postId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChannelPostDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
}



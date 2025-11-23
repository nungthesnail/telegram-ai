using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IPostService
{
    Task CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken cancellationToken);
    Task ScheduleAsync(Guid userId, Guid postId, SchedulePostRequest request, CancellationToken cancellationToken);
    Task PublishAsync(Guid userId, Guid postId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid postId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChannelPostDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
}



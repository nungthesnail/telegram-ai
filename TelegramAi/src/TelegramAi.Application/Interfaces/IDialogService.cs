using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IDialogService
{
    Task<DialogDto> StartAsync(Guid userId, CreateDialogRequest request, CancellationToken cancellationToken);
    Task<SendMessageResultDto> SendMessageAsync(Guid userId, AssistantMessageRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DialogDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DialogDto>> ListAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<DialogDto?> GetAsync(Guid userId, Guid dialogId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid dialogId, CancellationToken cancellationToken);
}



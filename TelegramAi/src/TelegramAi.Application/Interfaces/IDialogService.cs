using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface IDialogService
{
    Task<DialogDto> StartAsync(Guid userId, CreateDialogRequest request, CancellationToken cancellationToken);
    Task<AssistantResponseDto> SendMessageAsync(Guid userId, AssistantMessageRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DialogDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken);
}



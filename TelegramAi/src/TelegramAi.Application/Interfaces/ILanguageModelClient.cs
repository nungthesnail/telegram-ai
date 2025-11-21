using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILanguageModelClient
{
    Task<AssistantResponseDto> GenerateResponseAsync(
        Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken);
}



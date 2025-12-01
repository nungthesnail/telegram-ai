using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILanguageModelClient
{
    Task<AiResponseDto> GenerateResponseAsync(Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken);
}



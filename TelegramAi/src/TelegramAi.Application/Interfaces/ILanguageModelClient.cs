using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILanguageModelClient
{
    Task<AiResponseDto> GenerateResponseAsync(Guid dialogId,
        string modelId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        string? systemPrompt = null,
        string? toolsDescription = null,
        CancellationToken cancellationToken = default);
}



using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILanguageModelClient
{
    Task<string> GenerateResponseAsync(Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken);
}



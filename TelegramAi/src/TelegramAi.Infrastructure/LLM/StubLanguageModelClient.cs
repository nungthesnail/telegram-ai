using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Infrastructure.LLM;

public class StubLanguageModelClient : ILanguageModelClient
{
    public Task<AssistantResponseDto> GenerateResponseAsync(
        Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var responseText =
            $"(stub) Получил сообщение: \"{userMessage}\". Предлагаю подготовить пост и план действий.";

        var suggestedPosts = new List<ChannelPostDto>
        {
            new(
                Guid.NewGuid(),
                "Идея поста",
                $"Развиваем мысль: {userMessage}",
                Domain.Enums.ChannelPostStatus.Draft,
                DateTime.UtcNow,
                null,
                null)
        };

        return Task.FromResult(new AssistantResponseDto(responseText, suggestedPosts, "/publish"));
    }
}



using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Infrastructure.LLM;

public class StubLanguageModelClient : ILanguageModelClient
{
    public async Task<string> GenerateResponseAsync(Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken)
    {
        await Task.Delay(2000, cancellationToken);
        
        var responseText =
            $"(stub) Получил сообщение: \"{userMessage}\". Предлагаю подготовить пост и план действий.";

        var suggestedPosts = new List<ChannelPostDto>
        {
            new(
                Guid.NewGuid(),
                null,
                null,
                "Идея поста",
                $"Развиваем мысль: {userMessage}",
                Domain.Enums.ChannelPostStatus.Draft,
                DateTimeOffset.UtcNow,
                null,
                null)
        };

        return $"Создаю пост с предоставленным текстом. /publish {{ \"Title\": \"Тестовый пост\", \"Content\": \"{responseText.Replace("\"", "\\\"")}\"}}";
    }
}



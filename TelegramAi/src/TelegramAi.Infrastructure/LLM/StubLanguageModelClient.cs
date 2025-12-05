using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Infrastructure.LLM;

public class StubLanguageModelClient : ILanguageModelClient
{
    public async Task<AiResponseDto> GenerateResponseAsync(Guid dialogId,
        string modelId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        string? systemPrompt = null,
        string? toolsDescription = null,
        CancellationToken cancellationToken = default)
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

        return new AiResponseDto($"Создаю пост с предоставленным текстом. " +
               $"/publish {{ \"Title\": \"Тестовый пост 1\", \"Content\": \"{responseText.Replace("\"", "\\\"")}\"}}" +
               $"/publish {{ \"Title\": \"Второй тестовый пост\", \"Content\": \"{responseText.Replace("\"", "\\\"")}\"}}",
            new TokenUsageDto(1, 1, 2));
    }
}

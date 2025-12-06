using TelegramAi.Application.DTOs;
using TelegramAi.Application.DTOs.AiResponseEntities;
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
            $"Заглушка. Получено сообщение: \"{userMessage}\".";

        var posts = new List<ChannelPostDto>
        {
            new(Guid.Empty, null, null, "Идея поста", $"Развиваем мысль: {userMessage}",
                Domain.Enums.ChannelPostStatus.Draft, DateTimeOffset.UtcNow, null, null)
        };

        return new AiResponseDto([new TextMessageEntity(responseText), new SuggestedPostsMessageEntity(posts)],
            new TokenUsageDto(1, 1));
    }
}

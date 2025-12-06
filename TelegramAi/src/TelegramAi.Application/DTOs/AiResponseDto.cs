using System.Text.Json;
using TelegramAi.Application.DTOs.AiResponseEntities;

namespace TelegramAi.Application.DTOs;

public record AiResponseDto(
    IEnumerable<MessageEntity> ResponseEntities,
    TokenUsageDto TokenUsage)
{
    public string GetEntitiesJson()
    {
        return JsonSerializer.Serialize(ResponseEntities);
    }
}

public record TokenUsageDto(int InputTokenCount, int OutputTokenCount);

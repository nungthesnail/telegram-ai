namespace TelegramAi.Application.DTOs;

public record AiResponseDto(string Text, TokenUsageDto TokenUsage);

public record TokenUsageDto(int InputTokenCount, int OutputTokenCount, int TotalTokenCount);

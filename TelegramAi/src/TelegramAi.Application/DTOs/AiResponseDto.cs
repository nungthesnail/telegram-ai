namespace TelegramAi.Application.DTOs;

public record AiResponseDto(
    string Text, 
    TokenUsageDto TokenUsage,
    IReadOnlyCollection<ToolCallDto>? ToolCalls = null);

public record TokenUsageDto(int InputTokenCount, int OutputTokenCount, int TotalTokenCount);

public record ToolCallDto(
    string FunctionName,
    string Arguments);

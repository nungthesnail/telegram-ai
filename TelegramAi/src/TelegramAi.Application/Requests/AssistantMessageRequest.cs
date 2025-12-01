namespace TelegramAi.Application.Requests;

public record AssistantMessageRequest(Guid DialogId, long ModelId, string Message);

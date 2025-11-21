namespace TelegramAi.Application.Requests;

public record SendMessageRequest(
    Guid DialogId,
    string Message);



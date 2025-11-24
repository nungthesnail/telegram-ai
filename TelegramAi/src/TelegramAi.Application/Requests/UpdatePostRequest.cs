namespace TelegramAi.Application.Requests;

public record UpdatePostRequest(
    string Title,
    string Content,
    DateTimeOffset? ScheduledAtUtc);



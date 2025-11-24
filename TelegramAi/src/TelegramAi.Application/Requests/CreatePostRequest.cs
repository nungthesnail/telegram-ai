namespace TelegramAi.Application.Requests;

public record CreatePostRequest(
    Guid ChannelId,
    string Title,
    string Content,
    DateTimeOffset? ScheduledAtUtc);



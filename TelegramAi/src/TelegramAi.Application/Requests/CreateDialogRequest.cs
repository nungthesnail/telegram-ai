namespace TelegramAi.Application.Requests;

public record CreateDialogRequest(
    Guid ChannelId,
    string? Title,
    string? SystemPrompt);



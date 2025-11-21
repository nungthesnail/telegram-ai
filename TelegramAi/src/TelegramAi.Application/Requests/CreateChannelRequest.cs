namespace TelegramAi.Application.Requests;

public record CreateChannelRequest(
    string Title,
    string Description,
    string TelegramLink,
    string? Category);



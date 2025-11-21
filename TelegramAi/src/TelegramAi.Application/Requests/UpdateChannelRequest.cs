namespace TelegramAi.Application.Requests;

public record UpdateChannelRequest(
    string Title,
    string Description,
    string TelegramLink,
    string? Category);



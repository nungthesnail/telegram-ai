namespace TelegramAi.Application.DTOs;

public record ChannelDto(
    Guid Id,
    string Title,
    string Description,
    string TelegramLink,
    string? Category,
    bool BotLinked,
    ChannelBotLinkDto? Bot,
    IReadOnlyCollection<ChannelPostDto> Posts);



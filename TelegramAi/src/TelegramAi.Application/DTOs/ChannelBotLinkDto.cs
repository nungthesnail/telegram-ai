namespace TelegramAi.Application.DTOs;

public record ChannelBotLinkDto(
    Guid Id,
    string VerificationCode,
    long? TelegramChatId,
    long? TelegramBotId,
    DateTimeOffset? VerifiedAtUtc);



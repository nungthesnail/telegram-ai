namespace TelegramAi.Application.Requests;

public record ConfirmBotLinkRequest(
    Guid ChannelId,
    string VerificationCode,
    long TelegramChatId,
    long TelegramBotId);



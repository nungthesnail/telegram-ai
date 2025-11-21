namespace TelegramAi.Domain.Entities;

public class ChannelBotLink : BaseEntity
{
    public Guid ChannelId { get; set; }
    public long? TelegramChatId { get; set; }
    public long? TelegramBotId { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime? VerifiedAtUtc { get; set; }

    public Channel Channel { get; set; } = null!;
}



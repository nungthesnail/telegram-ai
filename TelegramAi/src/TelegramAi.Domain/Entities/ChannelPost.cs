using TelegramAi.Domain.Enums;

namespace TelegramAi.Domain.Entities;

public class ChannelPost : BaseEntity
{
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ChannelPostStatus Status { get; set; } = ChannelPostStatus.Draft;
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public string? ExternalMessageId { get; set; }

    public Channel Channel { get; set; } = null!;
}



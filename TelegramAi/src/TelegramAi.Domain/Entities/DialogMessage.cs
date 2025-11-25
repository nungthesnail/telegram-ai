using TelegramAi.Domain.Enums;

namespace TelegramAi.Domain.Entities;

public class DialogMessage : BaseEntity
{
    public Guid DialogId { get; set; }
    public DialogMessageSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? PostsJson { get; set; }

    public Dialog Dialog { get; set; } = null!;
}



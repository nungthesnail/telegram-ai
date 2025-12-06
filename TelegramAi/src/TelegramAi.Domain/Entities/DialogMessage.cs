using TelegramAi.Domain.Enums;

namespace TelegramAi.Domain.Entities;

public class DialogMessage : BaseEntity
{
    public Guid DialogId { get; set; }
    public DialogMessageSender Sender { get; set; }
    public string ContentEntitiesJson { get; set; } = string.Empty;

    public Dialog Dialog { get; set; } = null!;
}



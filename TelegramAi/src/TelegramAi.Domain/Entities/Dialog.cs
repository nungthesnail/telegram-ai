namespace TelegramAi.Domain.Entities;

public class Dialog : BaseEntity
{
    public Guid ChannelId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = "Новый диалог";
    public bool IsActive { get; set; } = true;

    public Channel Channel { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<DialogMessage> Messages { get; set; } = new List<DialogMessage>();
}



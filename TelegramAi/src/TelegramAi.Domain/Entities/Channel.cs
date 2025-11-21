namespace TelegramAi.Domain.Entities;

public class Channel : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TelegramLink { get; set; } = string.Empty;
    public string? Category { get; set; }

    public Guid? ActiveDialogId { get; set; }

    public User Owner { get; set; } = null!;
    public ChannelBotLink? BotLink { get; set; }
    public ICollection<Dialog> Dialogs { get; set; } = new List<Dialog>();
    public ICollection<ChannelPost> Posts { get; set; } = new List<ChannelPost>();
}



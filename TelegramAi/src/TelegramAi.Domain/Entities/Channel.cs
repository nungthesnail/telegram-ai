namespace TelegramAi.Domain.Entities;

public class Channel : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? TelegramLink { get; set; }
    public string? Category { get; set; }
    public string? AiDescription { get; set; }

    public User Owner { get; set; } = null!;
    public ChannelBotLink? BotLink { get; set; }
    public ICollection<Dialog> Dialogs { get; set; } = new List<Dialog>();
    public ICollection<ChannelPost> Posts { get; set; } = new List<ChannelPost>();
}



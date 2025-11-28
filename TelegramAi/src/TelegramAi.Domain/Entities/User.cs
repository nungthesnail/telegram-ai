namespace TelegramAi.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public long? TelegramUserId { get; set; }

    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    public ICollection<Dialog> Dialogs { get; set; } = new List<Dialog>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public UserSubscription? Subscription { get; set; }
}



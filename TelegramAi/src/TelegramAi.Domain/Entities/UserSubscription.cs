namespace TelegramAi.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTimeOffset LastRenewedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    
    public User User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}


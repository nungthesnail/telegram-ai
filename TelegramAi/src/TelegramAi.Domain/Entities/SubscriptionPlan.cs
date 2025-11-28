namespace TelegramAi.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PriceRub { get; set; }
    public int TokensPerPeriod { get; set; }
    public int PeriodDays { get; set; }
    
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}


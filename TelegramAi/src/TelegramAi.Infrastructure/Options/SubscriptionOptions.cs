namespace TelegramAi.Infrastructure.Options;

public class SubscriptionOptions
{
    public const string SectionName = "Subscription";
    public int DefaultTrialDays { get; set; } = 7;
    public decimal DefaultPrice { get; set; } = 990m;
    public string Currency { get; set; } = "RUB";
}



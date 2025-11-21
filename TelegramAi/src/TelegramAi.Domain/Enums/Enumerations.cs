namespace TelegramAi.Domain.Enums;

public enum SubscriptionStatus
{
    None = 0,
    Trial = 1,
    Active = 2,
    PastDue = 3,
    Cancelled = 4
}

public enum DialogMessageSender
{
    System = 0,
    User = 1,
    Assistant = 2
}

public enum ChannelPostStatus
{
    Draft = 0,
    Scheduled = 1,
    Published = 2,
    Failed = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Cancelled = 3
}

public enum PaymentProvider
{
    Stub = 0,
    Manual = 1,
    YooMoney = 2,
    Stripe = 3
}



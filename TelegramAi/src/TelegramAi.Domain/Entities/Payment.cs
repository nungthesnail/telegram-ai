using TelegramAi.Domain.Enums;

namespace TelegramAi.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentProvider Provider { get; set; } = PaymentProvider.Stub;
    public string? ExternalId { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public User User { get; set; } = null!;
}



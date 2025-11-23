namespace TelegramAi.Domain.Entities;

public class UserVerificationCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime? VerifiedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public User User { get; set; } = null!;
}


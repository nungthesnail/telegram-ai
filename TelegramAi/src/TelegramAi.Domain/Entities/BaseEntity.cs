using System.ComponentModel.DataAnnotations;

namespace TelegramAi.Domain.Entities;

public abstract class BaseEntity<TKey>
    where TKey : struct
{
    [Key]
    public TKey Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}

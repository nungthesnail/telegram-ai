namespace TelegramAi.Domain.Entities;

public class LlmModelInfo : BaseEntity<long>
{
    public required string Name { get; set; }
    public required string ApiId { get; set; }
    public decimal RequestTokenCost { get; set; }
    public decimal ResponseTokenCost { get; set; }
}

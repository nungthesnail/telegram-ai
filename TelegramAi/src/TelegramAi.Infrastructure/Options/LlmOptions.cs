namespace TelegramAi.Infrastructure.Options;

public class LlmOptions
{
    public const string SectionName = "Llm";
    public bool UseStub { get; set; } = true;
    public decimal TokenBudgetRatio { get; set; }
    public bool BalanceResetsWhenUpdating { get; set; }
}

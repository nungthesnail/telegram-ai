namespace TelegramAi.Infrastructure.Options;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = null!;
    public string ApiRoot { get; set; } = null!;
}



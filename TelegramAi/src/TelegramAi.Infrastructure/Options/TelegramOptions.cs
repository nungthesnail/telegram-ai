namespace TelegramAi.Infrastructure.Options;

public class TelegramOptions
{
    public const string SectionName = "Telegram";
    public string BotToken { get; set; } = string.Empty;
    public bool Enabled => !string.IsNullOrWhiteSpace(BotToken);
}



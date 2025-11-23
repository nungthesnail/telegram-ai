namespace TelegramAi.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TelegramAi";
    public string Audience { get; set; } = "TelegramAi";
}


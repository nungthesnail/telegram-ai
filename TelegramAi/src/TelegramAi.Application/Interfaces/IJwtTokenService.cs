namespace TelegramAi.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, DateTimeOffset? subscriptionExpiresAtUtc = null);
    bool ValidateToken(string token, out Guid? userId);
}


namespace TelegramAi.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email);
    bool ValidateToken(string token, out Guid? userId);
}


using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(string email, string displayName, string password, CancellationToken cancellationToken);
    Task<UserDto?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
    Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<string> GenerateVerificationCodeAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserDto> ConfirmTelegramUserAsync(Guid userId, string verificationCode, long telegramUserId, CancellationToken cancellationToken);
}



using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterOrUpdateAsync(string email, string displayName, CancellationToken cancellationToken);
    Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken);
}



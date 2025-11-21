using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDto> RegisterOrUpdateAsync(string email, string displayName, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = normalizedEmail,
                DisplayName = displayName,
                SubscriptionStatus = SubscriptionStatus.Trial,
                SubscriptionExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.Users.Add(user);
        }
        else
        {
            user.DisplayName = displayName;
            user.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return user?.ToDto();
    }
}



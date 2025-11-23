using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(string email, string displayName, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var passwordHash = HashPassword(password);
        
        var user = new User
        {
            Email = normalizedEmail,
            DisplayName = displayName,
            PasswordHash = passwordHash,
            SubscriptionStatus = SubscriptionStatus.Trial,
            SubscriptionExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User registered: {Email}", normalizedEmail);
        return user.ToDto();
    }

    public async Task<UserDto?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        
        if (user == null)
        {
            return null;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        _logger.LogInformation("User authenticated: {Email}", normalizedEmail);
        return user.ToDto();
    }

    public async Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return user?.ToDto();
    }

    public async Task<string> GenerateVerificationCodeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found");

        // Удаляем старые неиспользованные коды
        var expiredCodes = await _dbContext.UserVerificationCodes
            .Where(x => x.UserId == userId && (x.VerifiedAtUtc == null && x.ExpiresAtUtc < DateTime.UtcNow))
            .ToListAsync(cancellationToken);
        _dbContext.UserVerificationCodes.RemoveRange(expiredCodes);

        var verificationCode = GenerateVerificationCode();
        var codeEntity = new UserVerificationCode
        {
            UserId = userId,
            VerificationCode = verificationCode,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        };

        _dbContext.UserVerificationCodes.Add(codeEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated verification code for user {UserId}", userId);
        return verificationCode;
    }

    public async Task<UserDto> ConfirmTelegramUserAsync(Guid userId, string verificationCode, long telegramUserId, CancellationToken cancellationToken)
    {
        var codeEntity = await _dbContext.UserVerificationCodes
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.VerificationCode == verificationCode &&
                x.VerifiedAtUtc == null &&
                x.ExpiresAtUtc > DateTime.UtcNow, cancellationToken)
            ?? throw new InvalidOperationException("Verification code invalid or expired");

        // Проверяем, не используется ли уже этот TelegramUserId другим пользователем
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId && x.Id != userId, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("This Telegram account is already linked to another user");
        }

        codeEntity.User.TelegramUserId = telegramUserId;
        codeEntity.VerifiedAtUtc = DateTime.UtcNow;
        codeEntity.User.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} verified Telegram account {TelegramUserId}", userId, telegramUserId);

        return codeEntity.User.ToDto();
    }

    private static string GenerateVerificationCode()
    {
        Span<byte> bytes = stackalloc byte[5];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}



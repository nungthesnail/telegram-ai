using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    SubscriptionStatus SubscriptionStatus,
    DateTimeOffset? SubscriptionExpiresAtUtc,
    long? TelegramUserId);



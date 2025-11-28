namespace TelegramAi.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    long? TelegramUserId,
    UserSubscriptionDto? Subscription);



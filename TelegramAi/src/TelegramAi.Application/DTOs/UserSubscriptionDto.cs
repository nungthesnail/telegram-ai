namespace TelegramAi.Application.DTOs;

public record UserSubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid PlanId,
    DateTimeOffset LastRenewedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    SubscriptionPlanDto Plan);


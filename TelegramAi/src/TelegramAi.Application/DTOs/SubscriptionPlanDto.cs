namespace TelegramAi.Application.DTOs;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal PriceRub,
    int TokensPerPeriod,
    int PeriodDays);


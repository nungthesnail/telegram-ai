using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken);
    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken);
    Task RequestTelegramInvoiceAsync(Guid userId, Guid planId, CancellationToken cancellationToken);
    Task ProcessTelegramPaymentAsync(long telegramUserId, string telegramPaymentChargeId, decimal amount, 
        string currency, Guid planId, CancellationToken cancellationToken);
    Task UpdateBalanceAsync(Guid userId, LlmModelInfoDto modelInfo, TokenUsageDto tokenUsage,
        CancellationToken cancellationToken = default);
    SubscriptionStatusDto GetSubscriptionStatus(UserSubscriptionDto subscription,
        CancellationToken cancellationToken = default);
}

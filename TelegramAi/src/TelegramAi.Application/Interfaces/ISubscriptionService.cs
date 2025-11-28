using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken);
    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken);
    Task<PaymentDto> StartSubscriptionAsync(Guid userId, StartSubscriptionRequest request, CancellationToken cancellationToken);
    Task<UserDto> RefreshSubscriptionStateAsync(Guid userId, CancellationToken cancellationToken);
    Task RequestTelegramInvoiceAsync(Guid userId, Guid planId, CancellationToken cancellationToken);
    Task ProcessTelegramPaymentAsync(long telegramUserId, string telegramPaymentChargeId, decimal amount, string currency, Guid planId, CancellationToken cancellationToken);
}



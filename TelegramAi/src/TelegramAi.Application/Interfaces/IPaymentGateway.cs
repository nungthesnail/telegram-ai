using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.Interfaces;

public interface IPaymentGateway
{
    Task<(PaymentStatus Status, string ExternalPaymentId)> ChargeAsync(
        Guid userId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken);
}



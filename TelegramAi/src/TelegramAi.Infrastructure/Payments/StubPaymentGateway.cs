using Microsoft.Extensions.Logging;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Enums;

namespace TelegramAi.Infrastructure.Payments;

public class StubPaymentGateway : IPaymentGateway
{
    private readonly ILogger<StubPaymentGateway> _logger;

    public StubPaymentGateway(ILogger<StubPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<(PaymentStatus Status, string ExternalPaymentId)> ChargeAsync(
        Guid userId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken)
    {
        var externalId = $"stub_{Guid.NewGuid():N}";
        _logger.LogInformation("Simulated payment for user {UserId} amount {Amount} {Currency}", userId, amount, currency);
        return Task.FromResult((PaymentStatus.Paid, externalId));
    }
}



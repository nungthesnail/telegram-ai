using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Options;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOptions<SubscriptionOptions> _options;

    public SubscriptionService(AppDbContext dbContext, IPaymentGateway paymentGateway, IOptions<SubscriptionOptions> options)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
        _options = options;
    }

    public async Task<PaymentDto> StartSubscriptionAsync(Guid userId, StartSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found");

        var amount = request.Amount <= 0 ? _options.Value.DefaultPrice : request.Amount;
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? _options.Value.Currency : request.Currency;

        var payment = new Payment
        {
            UserId = userId,
            Amount = amount,
            Currency = currency,
            Provider = PaymentProvider.Stub,
            Status = PaymentStatus.Pending
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (status, externalId) = await _paymentGateway.ChargeAsync(userId, amount, currency, cancellationToken);

        payment.Status = status;
        payment.ExternalId = externalId;
        payment.PaidAtUtc = status == PaymentStatus.Paid ? DateTimeOffset.UtcNow : null;
        payment.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (status == PaymentStatus.Paid)
        {
            user.SubscriptionStatus = SubscriptionStatus.Active;
            user.SubscriptionExpiresAtUtc = DateTimeOffset.UtcNow.AddMonths(1);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return payment.ToDto();
    }

    public async Task<UserDto> RefreshSubscriptionStateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found");

        if (user.SubscriptionExpiresAtUtc is { } expires && expires < DateTimeOffset.UtcNow)
        {
            user.SubscriptionStatus = SubscriptionStatus.PastDue;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return user.ToDto();
    }
}



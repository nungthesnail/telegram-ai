using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class SubscriptionService(
    AppDbContext dbContext,
    IPaymentGateway paymentGateway,
    ITelegramPublisher telegramPublisher)
    : ISubscriptionService
{
    public async Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken)
    {
        var plans = await dbContext.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(x => x.PriceRub)
            .ToListAsync(cancellationToken);
        
        return plans.Select(x => x.ToDto());
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.UserSubscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        
        return subscription?.ToDto();
    }

    public async Task<PaymentDto> StartSubscriptionAsync(Guid userId, StartSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.Subscription)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found");

        // Получаем план подписки
        var planId = request.PlanId ?? throw new InvalidOperationException("PlanId is required");
        var plan = await dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription plan not found");

        var amount = plan.PriceRub;
        var currency = "RUB";

        var payment = new Payment
        {
            UserId = userId,
            Amount = amount,
            Currency = currency,
            Provider = PaymentProvider.Stub,
            Status = PaymentStatus.Pending
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (status, externalId) = await paymentGateway.ChargeAsync(userId, amount, currency, cancellationToken);

        payment.Status = status;
        payment.ExternalId = externalId;
        payment.PaidAtUtc = status == PaymentStatus.Paid ? DateTimeOffset.UtcNow : null;
        payment.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (status == PaymentStatus.Paid)
        {
            var now = DateTimeOffset.UtcNow;
            
            // Если у пользователя уже есть подписка, обновляем её, иначе создаём новую
            if (user.Subscription != null)
            {
                user.Subscription.PlanId = planId;
                user.Subscription.LastRenewedAtUtc = now;
                user.Subscription.ExpiresAtUtc = now.AddDays(plan.PeriodDays);
                user.Subscription.UpdatedAtUtc = now;
            }
            else
            {
                user.Subscription = new UserSubscription
                {
                    UserId = userId,
                    PlanId = planId,
                    LastRenewedAtUtc = now,
                    ExpiresAtUtc = now.AddDays(plan.PeriodDays)
                };
                dbContext.UserSubscriptions.Add(user.Subscription);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return payment.ToDto();
    }

    public async Task<UserDto> RefreshSubscriptionStateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.Subscription)
                .ThenInclude(x => x!.Plan)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found");

        // Проверка истечения подписки выполняется на уровне бизнес-логики при проверке доступа
        // Здесь просто возвращаем актуальное состояние пользователя

        return user.ToDto();
    }

    public async Task RequestTelegramInvoiceAsync(Guid userId, Guid planId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found");

        if (user.TelegramUserId == null)
        {
            throw new InvalidOperationException("User must have linked Telegram account");
        }

        var plan = await dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription plan not found");

        // Отправляем invoice
        await telegramPublisher.SendInvoiceAsync(user.TelegramUserId.Value, plan, cancellationToken);

        // Создаем запись о платеже в статусе Pending
        var payment = new Payment
        {
            UserId = userId,
            Amount = plan.PriceRub,
            Currency = "RUB",
            Provider = PaymentProvider.Stub, // Будет обновлено при успешной оплате
            Status = PaymentStatus.Pending,
            ExternalId = $"telegram_invoice_{planId}"
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessTelegramPaymentAsync(long telegramUserId, string telegramPaymentChargeId, decimal amount, string currency, Guid planId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.Subscription)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found");

        var plan = await dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription plan not found");

        // Проверяем сумму
        if (Math.Abs(amount - plan.PriceRub) > 0.01m)
        {
            throw new InvalidOperationException("Payment amount does not match plan price");
        }

        var now = DateTimeOffset.UtcNow;

        // Создаем или обновляем подписку
        if (user.Subscription != null)
        {
            user.Subscription.PlanId = planId;
            user.Subscription.LastRenewedAtUtc = now;
            user.Subscription.ExpiresAtUtc = now.AddDays(plan.PeriodDays);
            user.Subscription.UpdatedAtUtc = now;
        }
        else
        {
            user.Subscription = new UserSubscription
            {
                UserId = user.Id,
                PlanId = planId,
                LastRenewedAtUtc = now,
                ExpiresAtUtc = now.AddDays(plan.PeriodDays)
            };
            dbContext.UserSubscriptions.Add(user.Subscription);
        }

        // Создаем запись о платеже
        var payment = new Payment
        {
            UserId = user.Id,
            Amount = amount,
            Currency = currency,
            Provider = PaymentProvider.Stub, // Telegram Payments
            Status = PaymentStatus.Paid,
            ExternalId = telegramPaymentChargeId,
            PaidAtUtc = now
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}



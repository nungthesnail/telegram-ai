using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Options;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class SubscriptionService(
    AppDbContext dbContext,
    ITelegramPublisher telegramPublisher,
    IOptionsMonitor<LlmOptions> llmOptions)
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
        var balance = amount * llmOptions.CurrentValue.TokenBudgetRatio;

        // Создаем или обновляем подписку
        if (user.Subscription != null)
        {
            user.Subscription.PlanId = planId;
            user.Subscription.LastRenewedAtUtc = now;
            user.Subscription.ExpiresAtUtc = now.AddDays(plan.PeriodDays);
            user.Subscription.UpdatedAtUtc = now;
            user.Subscription.Balance = llmOptions.CurrentValue.BalanceResetsWhenUpdating
                ? balance : user.Subscription.Balance + balance;
            user.Subscription.LastReplenishAmount = user.Subscription.Balance;
        }
        else
        {
            user.Subscription = new UserSubscription
            {
                UserId = user.Id,
                PlanId = planId,
                LastRenewedAtUtc = now,
                ExpiresAtUtc = now.AddDays(plan.PeriodDays),
                Balance = balance,
                LastReplenishAmount = balance
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

    public Task UpdateBalanceAsync(Guid userId, LlmModelInfoDto modelInfo, TokenUsageDto tokenUsage,
        CancellationToken cancellationToken = default)
    {
        return dbContext.UserSubscriptions.Where(x => x.UserId == userId).ExecuteUpdateAsync(
            setter => setter.SetProperty(x => x.Balance, x => x.Balance -
                (modelInfo.RequestTokenCost / 1000000 * tokenUsage.InputTokenCount
                + modelInfo.ResponseTokenCost / 1000000 * tokenUsage.OutputTokenCount)),
            cancellationToken); // Update balance
    }

    public SubscriptionStatusDto GetSubscriptionStatus(UserSubscriptionDto subscription,
        CancellationToken cancellationToken = default)
    {
        return new SubscriptionStatusDto(subscription.Balance <= 0, DateTimeOffset.Now > subscription.ExpiresAtUtc);
    }
}

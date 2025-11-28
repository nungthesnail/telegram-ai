namespace TelegramAi.Application.Requests;

public record ProcessTelegramPaymentRequest(
    long TelegramUserId,
    string TelegramPaymentChargeId,
    decimal Amount,
    string Currency,
    Guid PlanId);


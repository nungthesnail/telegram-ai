namespace TelegramAi.Application.Requests;

public record StartSubscriptionRequest(
    string PlanCode,
    int TrialDays,
    decimal Amount,
    string Currency);



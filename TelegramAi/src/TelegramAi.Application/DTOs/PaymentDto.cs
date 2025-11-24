using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record PaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    PaymentProvider Provider,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PaidAtUtc,
    string? ExternalId);



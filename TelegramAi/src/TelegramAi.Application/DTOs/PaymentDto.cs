using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record PaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    PaymentProvider Provider,
    DateTime CreatedAtUtc,
    DateTime? PaidAtUtc,
    string? ExternalId);



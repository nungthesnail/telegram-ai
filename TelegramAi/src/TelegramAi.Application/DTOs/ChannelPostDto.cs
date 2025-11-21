using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record ChannelPostDto(
    Guid Id,
    string Title,
    string Content,
    ChannelPostStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ScheduledAtUtc,
    DateTime? PublishedAtUtc);



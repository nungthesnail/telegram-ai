using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record DialogMessageDto(
    Guid Id,
    DialogMessageSender Sender,
    string Content,
    DateTimeOffset CreatedAtUtc,
    List<ChannelPostDto>? SuggestedPosts = null);
    
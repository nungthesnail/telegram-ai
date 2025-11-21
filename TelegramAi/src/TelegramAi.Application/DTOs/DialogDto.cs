namespace TelegramAi.Application.DTOs;

public record DialogDto(
    Guid Id,
    Guid ChannelId,
    Guid UserId,
    string Title,
    bool IsActive,
    IReadOnlyCollection<DialogMessageDto> Messages);



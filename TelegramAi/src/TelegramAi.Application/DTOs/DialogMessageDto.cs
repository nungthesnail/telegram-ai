using TelegramAi.Application.DTOs.AiResponseEntities;
using TelegramAi.Domain.Enums;

namespace TelegramAi.Application.DTOs;

public record DialogMessageDto(
    Guid Id,
    DialogMessageSender Sender,
    IEnumerable<MessageEntity> Entities,
    DateTimeOffset CreatedAtUtc)
{
    public string AsText => string.Join(' ',
        Entities.Where(x => x is TextMessageEntity).Select(x => ((TextMessageEntity)x).Text));
}

using System.Diagnostics.CodeAnalysis;

namespace TelegramAi.Application.DTOs.AiResponseEntities;

public class ErrorMessageEntity : MessageEntity
{
    public required string Error { get; set; }

    public ErrorMessageEntity()
    { }

    [SetsRequiredMembers]
    public ErrorMessageEntity(string error) => Error = error;
}

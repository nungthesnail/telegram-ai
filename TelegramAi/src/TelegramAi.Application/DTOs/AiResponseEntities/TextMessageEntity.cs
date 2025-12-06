using System.Diagnostics.CodeAnalysis;

namespace TelegramAi.Application.DTOs.AiResponseEntities;

public class TextMessageEntity : MessageEntity
{
    public required string Text { get; set; }

    public TextMessageEntity()
    { }
    
    [SetsRequiredMembers]
    public TextMessageEntity(string text) => Text = text;
}

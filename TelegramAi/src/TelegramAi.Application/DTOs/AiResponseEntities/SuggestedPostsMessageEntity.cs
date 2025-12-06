using System.Diagnostics.CodeAnalysis;

namespace TelegramAi.Application.DTOs.AiResponseEntities;

public class SuggestedPostsMessageEntity : MessageEntity
{
    public required IEnumerable<ChannelPostDto> Posts { get; set; }

    public SuggestedPostsMessageEntity()
    { }

    [SetsRequiredMembers]
    public SuggestedPostsMessageEntity(IEnumerable<ChannelPostDto> posts) => Posts = posts;
}

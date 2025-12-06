using System.Text.Json.Serialization;

namespace TelegramAi.Application.DTOs.AiResponseEntities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextMessageEntity), "text")]
[JsonDerivedType(typeof(SuggestedPostsMessageEntity), "suggestedPosts")]
[JsonDerivedType(typeof(ErrorMessageEntity), "error")]
public abstract class MessageEntity;

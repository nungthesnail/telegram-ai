namespace TelegramAi.Application.DTOs;

public record SendMessageResultDto(
    DialogMessageDto UserMessage,
    DialogMessageDto? AssistantResponse,
    List<ChannelPostDto>? SuggestedPosts);

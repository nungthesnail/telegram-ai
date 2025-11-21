namespace TelegramAi.Application.DTOs;

public record AssistantResponseDto(
    string AssistantMessage,
    IReadOnlyCollection<ChannelPostDto> SuggestedPosts,
    string? ProposedCommand);



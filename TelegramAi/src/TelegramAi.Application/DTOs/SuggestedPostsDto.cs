using System.Collections.Generic;

namespace TelegramAi.Application.DTOs;

public record SuggestedPostsDto(IReadOnlyCollection<ChannelPostDto> Posts);

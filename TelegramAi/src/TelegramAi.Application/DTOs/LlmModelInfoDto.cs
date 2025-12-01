namespace TelegramAi.Application.DTOs;

public record LlmModelInfoDto(long Id, string Name, string ApiId, decimal RequestTokenCost, decimal ResponseTokenCost);

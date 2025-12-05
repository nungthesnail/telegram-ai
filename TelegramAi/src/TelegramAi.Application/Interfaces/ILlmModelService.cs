using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILlmModelService
{
    Task<LlmModelInfoDto?> GetModelInfoAsync(long id, CancellationToken stoppingToken = default);
    Task<string> GetSystemPromptAsync(CancellationToken cancellationToken = default);
    Task<string> GetToolsDescriptionAsync(CancellationToken cancellationToken = default);
}

using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

public interface ILlmModelService
{
    Task<LlmModelInfoDto?> GetModelInfoAsync(long id, CancellationToken stoppingToken = default);
}

using TelegramAi.Application.DTOs;

namespace TelegramAi.Application.Interfaces;

/// <summary>
/// Интерфейс для выполнения инструментов, вызываемых LLM
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Выполняет инструмент и возвращает результат в формате JSON
    /// </summary>
    /// <param name="functionName">Название функции</param>
    /// <param name="arguments">Аргументы функции в формате JSON</param>
    /// <param name="dialogId">ID диалога</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат выполнения в формате JSON</returns>
    Task<string> ExecuteToolAsync(string functionName, string arguments, Guid dialogId, CancellationToken cancellationToken);
}


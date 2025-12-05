using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class LlmModelService : ILlmModelService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<long, LlmModelInfoDto> _cachedInfo = new();
    private readonly Lazy<string> _systemPrompt;
    private readonly Lazy<string> _toolsDescription;
    
    public LlmModelService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _systemPrompt = new Lazy<string>(() => LoadResourceFile("SystemPrompt.txt"));
        _toolsDescription = new Lazy<string>(() => LoadResourceFile("ToolsDescription.json"));
    }
    
    public async Task<LlmModelInfoDto?> GetModelInfoAsync(long id, CancellationToken stoppingToken)
    {
        if (_cachedInfo.TryGetValue(id, out var found))
            return found;
        
        await using var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var info = (await dbCtx.LlmModelInfos.FirstOrDefaultAsync(x => x.Id == id, stoppingToken))?.ToDto();
        if (info is not null)
            _cachedInfo[id] = info;
        return info;
    }

    public Task<string> GetSystemPromptAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_systemPrompt.Value);
    }

    public Task<string> GetToolsDescriptionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toolsDescription.Value);
    }

    private static string LoadResourceFile(string fileName)
    {
        var filePath = $"LLM/{fileName}";
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        throw new FileNotFoundException($"File {filePath} not found");
    }
}

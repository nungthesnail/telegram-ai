using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class LlmModelService(IServiceProvider serviceProvider) : ILlmModelService
{
    private readonly ConcurrentDictionary<long, LlmModelInfoDto> _cachedInfo = new();
    
    public async Task<LlmModelInfoDto?> GetModelInfoAsync(long id, CancellationToken stoppingToken)
    {
        if (_cachedInfo.TryGetValue(id, out var found))
            return found;
        
        await using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var info = (await dbCtx.LlmModelInfos.FirstOrDefaultAsync(x => x.Id == id, stoppingToken))?.ToDto();
        if (info is not null)
            _cachedInfo[id] = info;
        return info;
    }
}

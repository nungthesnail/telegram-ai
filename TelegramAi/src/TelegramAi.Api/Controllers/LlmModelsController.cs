using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/llm-models")]
[Authorize]
public class LlmModelsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public LlmModelsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LlmModelInfoDto>>> List(CancellationToken cancellationToken)
    {
        var models = await _dbContext.LlmModelInfos
            .AsNoTracking()
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
        
        return Ok(models);
    }
}


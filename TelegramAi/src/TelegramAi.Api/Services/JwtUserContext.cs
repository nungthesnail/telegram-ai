using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Services;

public class JwtUserContext : IUserContext
{
    private static readonly Guid DemoUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : DemoUserId;
    }
}


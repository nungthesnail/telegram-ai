using Microsoft.AspNetCore.Http;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Services;

public class HeaderUserContext : IUserContext
{
    private const string HeaderName = "X-User-Id";
    private static readonly Guid DemoUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
        return Guid.TryParse(headerValue, out var userId) ? userId : DemoUserId;
    }
}



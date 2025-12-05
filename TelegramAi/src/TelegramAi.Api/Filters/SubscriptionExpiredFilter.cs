using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Filters;

public class SubscriptionExpiredFilter : IAsyncActionFilter
{
    private readonly IUserService _userService;

    public SubscriptionExpiredFilter(IUserService userService)
    {
        _userService = userService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            await next();
            return;
        }

        var subscriptionExpiresClaim = context.HttpContext.User.FindFirst("subscription_expires_at");
        if (subscriptionExpiresClaim != null && long.TryParse(subscriptionExpiresClaim.Value, out var expiresUnix))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
            if (expiresAt < DateTimeOffset.UtcNow)
            {
                context.Result = new ObjectResult(new { message = "Subscription expired" })
                {
                    StatusCode = 402
                };
                return;
            }
        }
        else
        {
            // Если claim нет, проверяем в базе данных
            var user = await _userService.GetAsync(userId, context.HttpContext.RequestAborted);
            if (user?.Subscription == null || user.Subscription.ExpiresAtUtc < DateTimeOffset.UtcNow)
            {
                context.Result = new ObjectResult(new { message = "Subscription expired" })
                {
                    StatusCode = 402
                };
                return;
            }
        }

        await next();
    }
}


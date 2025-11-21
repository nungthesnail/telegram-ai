using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/subscription")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUserContext _userContext;

    public SubscriptionController(ISubscriptionService subscriptionService, IUserContext userContext)
    {
        _subscriptionService = subscriptionService;
        _userContext = userContext;
    }

    [HttpPost("start")]
    public async Task<ActionResult<PaymentDto>> Start([FromBody] StartSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var payment = await _subscriptionService.StartSubscriptionAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return Ok(payment);
    }

    [HttpGet("state")]
    public async Task<ActionResult<UserDto>> GetState(CancellationToken cancellationToken)
    {
        var state = await _subscriptionService.RefreshSubscriptionStateAsync(_userContext.GetCurrentUserId(), cancellationToken);
        return Ok(state);
    }
}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUserContext _userContext;

    public SubscriptionController(ISubscriptionService subscriptionService, IUserContext userContext)
    {
        _subscriptionService = subscriptionService;
        _userContext = userContext;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _subscriptionService.GetPlansAsync(cancellationToken);
        return Ok(plans);
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserSubscriptionDto>> GetCurrent(CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetUserSubscriptionAsync(_userContext.GetCurrentUserId(), cancellationToken);
        return subscription is null ? NotFound() : Ok(subscription);
    }

    [HttpPost("start")]
    public async Task<ActionResult<PaymentDto>> Start([FromBody] StartSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var payment = await _subscriptionService.StartSubscriptionAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("request-telegram-invoice")]
    public async Task<ActionResult> RequestTelegramInvoice([FromBody] RequestTelegramInvoiceRequest request, CancellationToken cancellationToken)
    {
        await _subscriptionService.RequestTelegramInvoiceAsync(_userContext.GetCurrentUserId(), request.PlanId, cancellationToken);
        return Ok(new { message = "Invoice sent to Telegram" });
    }
}



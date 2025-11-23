using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/channels")]
[Authorize]
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly IUserContext _userContext;

    public ChannelsController(IChannelService channelService, IUserContext userContext)
    {
        _channelService = channelService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ChannelDto>>> List(CancellationToken cancellationToken)
    {
        var channels = await _channelService.ListAsync(_userContext.GetCurrentUserId(), cancellationToken);
        return Ok(channels);
    }

    [HttpGet("{channelId:guid}")]
    public async Task<ActionResult<ChannelDto>> Get(Guid channelId, CancellationToken cancellationToken)
    {
        var channel = await _channelService.GetAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return channel is null ? NotFound() : Ok(channel);
    }

    [HttpPost]
    public async Task<ActionResult<ChannelDto>> Create([FromBody] CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = await _channelService.CreateAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { channelId = channel.Id }, channel);
    }

    [HttpPut("{channelId:guid}")]
    public async Task<ActionResult<ChannelDto>> Update(Guid channelId, [FromBody] UpdateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = await _channelService.UpdateAsync(_userContext.GetCurrentUserId(), channelId, request, cancellationToken);
        return Ok(channel);
    }

    [HttpPost("{channelId:guid}/confirm")]
    public async Task<ActionResult<ChannelDto>> ConfirmChannelLink(Guid channelId, CancellationToken cancellationToken)
    {
        var channel = await _channelService.ConfirmChannelLinkAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return Ok(channel);
    }

    [HttpPut("{channelId:guid}/ai-description")]
    public async Task<ActionResult<ChannelDto>> UpdateAiDescription(Guid channelId, [FromBody] UpdateAiDescriptionRequest request, CancellationToken cancellationToken)
    {
        var channel = await _channelService.UpdateAiDescriptionAsync(_userContext.GetCurrentUserId(), channelId, request.AiDescription, cancellationToken);
        return Ok(channel);
    }

    [HttpDelete("{channelId:guid}")]
    public async Task<ActionResult> Delete(Guid channelId, CancellationToken cancellationToken)
    {
        await _channelService.DeleteAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return NoContent();
    }
}



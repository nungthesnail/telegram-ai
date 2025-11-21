using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/channels")]
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

    [HttpPost("{channelId:guid}/bot/request")]
    public async Task<ActionResult<ChannelBotLinkDto>> RequestBotLink(Guid channelId, CancellationToken cancellationToken)
    {
        var result = await _channelService.RequestBotLinkAsync(_userContext.GetCurrentUserId(), new RequestBotLinkRequest(channelId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{channelId:guid}/bot/confirm")]
    public async Task<ActionResult<ChannelBotLinkDto>> ConfirmBotLink(Guid channelId, [FromBody] ConfirmBotLinkRequest request, CancellationToken cancellationToken)
    {
        if (channelId != request.ChannelId)
        {
            return BadRequest("Channel id mismatch");
        }

        var result = await _channelService.ConfirmBotLinkAsync(request, cancellationToken);
        return Ok(result);
    }
}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/posts")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IUserContext _userContext;

    public PostsController(IPostService postService, IUserContext userContext)
    {
        _postService = postService;
        _userContext = userContext;
    }

    [HttpGet("/api/channels/{channelId:guid}/posts")]
    public async Task<ActionResult<IReadOnlyCollection<ChannelPostDto>>> List(Guid channelId, CancellationToken cancellationToken)
    {
        var posts = await _postService.ListByChannelAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return Ok(posts);
    }

    [HttpPost]
    public async Task<ActionResult<ChannelPostDto>> Create([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await _postService.CreateAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return Ok(post);
    }

    [HttpPut("{postId:guid}")]
    public async Task<ActionResult<ChannelPostDto>> Update(Guid postId, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await _postService.UpdateAsync(_userContext.GetCurrentUserId(), postId, request, cancellationToken);
        return Ok(post);
    }

    [HttpPost("{postId:guid}/schedule")]
    public async Task<ActionResult<ChannelPostDto>> Schedule(Guid postId, [FromBody] SchedulePostRequest request, CancellationToken cancellationToken)
    {
        var post = await _postService.ScheduleAsync(_userContext.GetCurrentUserId(), postId, request, cancellationToken);
        return Ok(post);
    }

    [HttpPost("{postId:guid}/publish")]
    public async Task<ActionResult<ChannelPostDto>> Publish(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postService.PublishAsync(_userContext.GetCurrentUserId(), postId, cancellationToken);
        return Ok(post);
    }
}



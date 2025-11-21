using Microsoft.AspNetCore.Mvc;
using TelegramAi.Api.Models;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/dialogs")]
public class DialogsController : ControllerBase
{
    private readonly IDialogService _dialogService;
    private readonly IUserContext _userContext;

    public DialogsController(IDialogService dialogService, IUserContext userContext)
    {
        _dialogService = dialogService;
        _userContext = userContext;
    }

    [HttpGet("/api/channels/{channelId:guid}/dialogs")]
    public async Task<ActionResult<IReadOnlyCollection<DialogDto>>> List(Guid channelId, CancellationToken cancellationToken)
    {
        var dialogs = await _dialogService.ListByChannelAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return Ok(dialogs);
    }

    [HttpPost]
    public async Task<ActionResult<DialogDto>> Start([FromBody] CreateDialogRequest request, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.StartAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(List), new { channelId = request.ChannelId }, dialog);
    }

    [HttpPost("{dialogId:guid}/messages")]
    public async Task<ActionResult<AssistantResponseDto>> SendMessage(Guid dialogId, [FromBody] SendDialogMessageRequest request, CancellationToken cancellationToken)
    {
        var response = await _dialogService.SendMessageAsync(
            _userContext.GetCurrentUserId(),
            new AssistantMessageRequest(dialogId, request.Message),
            cancellationToken);

        return Ok(response);
    }
}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Api.Models;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/dialogs")]
[Authorize]
public class DialogsController : ControllerBase
{
    private readonly IDialogService _dialogService;
    private readonly IUserContext _userContext;

    public DialogsController(IDialogService dialogService, IUserContext userContext)
    {
        _dialogService = dialogService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DialogDto>>> ListAll(CancellationToken cancellationToken)
    {
        var dialogs = await _dialogService.ListAllAsync(_userContext.GetCurrentUserId(), cancellationToken);
        return Ok(dialogs);
    }

    [HttpGet("/api/channels/{channelId:guid}/dialogs")]
    public async Task<ActionResult<IReadOnlyCollection<DialogDto>>> ListByChannel(Guid channelId, CancellationToken cancellationToken)
    {
        var dialogs = await _dialogService.ListByChannelAsync(_userContext.GetCurrentUserId(), channelId, cancellationToken);
        return Ok(dialogs);
    }

    [HttpPost]
    public async Task<ActionResult<DialogDto>> Start([FromBody] CreateDialogRequest request, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.StartAsync(_userContext.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { dialogId = dialog.Id }, dialog);
    }

    [HttpGet("{dialogId:guid}")]
    public async Task<ActionResult<DialogDto>> Get(Guid dialogId, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.GetAsync(_userContext.GetCurrentUserId(), dialogId, cancellationToken);
        return dialog is null ? NotFound() : Ok(dialog);
    }

    [HttpPost("{dialogId:guid}/messages")]
    public async Task<ActionResult<SendMessageResultDto>> SendMessage(Guid dialogId, [FromBody] SendDialogMessageRequest request, CancellationToken cancellationToken)
    {
        var response = await _dialogService.SendMessageAsync(
            _userContext.GetCurrentUserId(),
            new AssistantMessageRequest(dialogId, request.ModelId, request.Message),
            cancellationToken);
        
        return Ok(response);
    }

    [HttpDelete("{dialogId:guid}")]
    public async Task<ActionResult> Delete(Guid dialogId, CancellationToken cancellationToken)
    {
        await _dialogService.DeleteAsync(_userContext.GetCurrentUserId(), dialogId, cancellationToken);
        return NoContent();
    }
}



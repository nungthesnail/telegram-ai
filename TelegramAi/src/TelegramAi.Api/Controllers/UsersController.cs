using Microsoft.AspNetCore.Mvc;
using TelegramAi.Api.Models;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UsersController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    [HttpPost("ensure")]
    public async Task<ActionResult<UserDto>> Ensure([FromBody] EnsureUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.RegisterOrUpdateAsync(request.Email, request.DisplayName, cancellationToken);
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        var user = await _userService.GetAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}



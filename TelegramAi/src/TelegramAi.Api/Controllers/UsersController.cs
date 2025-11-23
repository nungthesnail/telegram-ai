using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UsersController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        var user = await _userService.GetAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("verification-code")]
    public async Task<ActionResult<string>> GenerateVerificationCode(CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        var code = await _userService.GenerateVerificationCodeAsync(userId, cancellationToken);
        return Ok(new { verificationCode = code });
    }
}



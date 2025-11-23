using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;

namespace TelegramAi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IUserService userService, IJwtTokenService jwtTokenService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return BadRequest("Password must be at least 6 characters long");
        }

        try
        {
            var user = await _userService.RegisterAsync(request.Email, request.DisplayName, request.Password, cancellationToken);
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email);

            return Ok(new AuthResponseDto
            {
                User = user,
                Token = token
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        var user = await _userService.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        
        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);

        return Ok(new AuthResponseDto
        {
            User = user,
            Token = token
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _userService.GetAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}

public record RegisterRequest(string Email, string DisplayName, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponseDto
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = null!;
}


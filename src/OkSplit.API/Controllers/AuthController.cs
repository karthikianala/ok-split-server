using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.DTOs.Auth;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var user = await _authService.GetMeAsync(userId);
        return Ok(new { user });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _authService.UpdateProfileAsync(userId, dto);
        return Ok(new { user });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}

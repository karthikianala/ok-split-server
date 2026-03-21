using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OkSplit.Application.DTOs.Auth;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Entities;

namespace OkSplit.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(UserManager<User> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ArgumentException("A user with this email already exists.");

        var user = new User
        {
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName,
            Phone = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Registration failed: {errors}");
        }

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

        if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return await GenerateAuthResponse(user);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task<UserResponseDto> GetMeAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        return _mapper.Map<UserResponseDto>(user);
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            User = _mapper.Map<UserResponseDto>(user),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}

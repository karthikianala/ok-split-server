using OkSplit.Application.DTOs.Auth;

namespace OkSplit.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task LogoutAsync(Guid userId);
    Task<UserResponseDto> GetMeAsync(Guid userId);
    Task<UserResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}

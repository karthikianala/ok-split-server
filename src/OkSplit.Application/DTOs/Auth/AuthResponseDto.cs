namespace OkSplit.Application.DTOs.Auth;

public class AuthResponseDto
{
    public UserResponseDto User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

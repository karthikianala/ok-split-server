using OkSplit.Domain.Entities;

namespace OkSplit.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

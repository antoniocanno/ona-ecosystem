using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IRefreshTokenService : ITokenService<RefreshToken>
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
    }
}

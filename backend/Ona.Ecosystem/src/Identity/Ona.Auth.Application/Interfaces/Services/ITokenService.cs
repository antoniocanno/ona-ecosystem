using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface ITokenService<T> where T : BaseToken
    {
        Task<T> GetByTokenAsync(string token);
        Task<T> CreateAsync(Guid userId, TimeSpan validityDuration, int tokenLength = 32);
        Task<T> CreateAsync(T token);
        Task RevokeTokenAsync(string token);
        Task RevokeAllUserTokensAsync(Guid userId);
        Task<int> GetUserTokenCountAsync(Guid userId);
    }
}
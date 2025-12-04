using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface ITokenRepository<T> : IBaseRepository<T> where T : BaseToken
    {
        Task<T?> GetByTokenAsync(string token);
        Task RevokeAllUserTokensAsync(string userId);
        Task CleanupExpiredTokensAsync();
        Task<int> GetUserTokenCountAsync(string userId);
    }
}

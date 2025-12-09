using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class TokenRepository<T> : BaseRepository<AuthDbContext, T>, ICleanupableTokenRepository, ITokenRepository<T> where T : BaseToken
    {
        public TokenRepository(AuthDbContext authDbContext)
            : base(authDbContext) { }

        public async Task<T?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            await _dbSet
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.IsRevoked, true)
                    .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                );
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var auditDays = DateTime.UtcNow.AddDays(-7);

            await _dbSet
                .Where(t =>
                    t.ExpiresAt < auditDays ||
                    t.IsRevoked && t.RevokedAt.HasValue && t.RevokedAt.Value < auditDays
                )
                .ExecuteDeleteAsync();
        }

        public async Task<int> GetUserTokenCountAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(rt => rt.UserId == userId && !rt.IsRevoked);
        }
    }
}

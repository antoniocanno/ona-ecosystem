using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class TokenRepository<T> : BaseRepository<T>, ICleanupableTokenRepository, ITokenRepository<T> where T : BaseToken
    {
        public TokenRepository(AuthDbContext authDbContext)
            : base(authDbContext) { }

        public async Task<T?> GetByTokenAsync(string token)
        {
            return await DbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            await DbSet
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.IsRevoked, true)
                    .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                );
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var auditDays = DateTime.UtcNow.AddDays(-7);

            await DbSet
                .Where(t =>
                    t.ExpiresAt < auditDays ||
                    t.IsRevoked && t.RevokedAt.HasValue && t.RevokedAt.Value < auditDays
                )
                .ExecuteDeleteAsync();
        }

        public async Task<int> GetUserTokenCountAsync(string userId)
        {
            return await DbSet
                .CountAsync(rt => rt.UserId == userId && !rt.IsRevoked);
        }
    }
}

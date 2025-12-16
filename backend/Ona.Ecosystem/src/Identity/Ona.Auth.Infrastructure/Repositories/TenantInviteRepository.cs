using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class TenantInviteRepository : BaseRepository<AuthDbContext, TenantInvite>, ITenantInviteRepository
    {
        public TenantInviteRepository(AuthDbContext authDbContext) : base(authDbContext) { }

        public async Task<TenantInvite?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}

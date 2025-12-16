using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class ApplicationRoleRepository : BaseRepository<AuthDbContext, ApplicationRole>, IApplicationRoleRepository
    {
        public ApplicationRoleRepository(AuthDbContext authDbContext) : base(authDbContext)
        {
        }

        public async Task<ApplicationRole?> GetByNameAndTenantAsync(string roleName, Guid tenantId)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.NormalizedName == roleName.ToUpperInvariant());
        }
    }
}

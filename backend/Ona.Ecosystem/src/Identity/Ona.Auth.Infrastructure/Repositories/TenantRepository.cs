using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Infrastructure.Data;
using Ona.Core.Entities;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class TenantRepository : BaseRepository<AuthDbContext, Tenant>, ITenantRepository
    {
        public TenantRepository(AuthDbContext authDbContext)
            : base(authDbContext) { }
    }
}

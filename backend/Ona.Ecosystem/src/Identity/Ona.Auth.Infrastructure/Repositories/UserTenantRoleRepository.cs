using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class UserTenantRoleRepository : BaseRepository<AuthDbContext, UserTenantRole>, IUserTenantRoleRepository
    {
        public UserTenantRoleRepository(AuthDbContext authDbContext) : base(authDbContext)
        {
        }
    }
}

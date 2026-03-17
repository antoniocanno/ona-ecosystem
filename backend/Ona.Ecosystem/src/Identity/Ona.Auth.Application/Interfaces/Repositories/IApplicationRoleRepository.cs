using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface IApplicationRoleRepository : IBaseRepository<ApplicationRole>
    {
        Task<ApplicationRole?> GetByNameAndTenantAsync(string roleName, Guid tenantId);
    }
}

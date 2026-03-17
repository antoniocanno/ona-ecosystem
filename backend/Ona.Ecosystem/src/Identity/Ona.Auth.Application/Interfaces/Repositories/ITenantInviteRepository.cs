using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface ITenantInviteRepository : IBaseRepository<TenantInvite>
    {
        Task<TenantInvite?> GetByTokenAsync(string token);
    }
}

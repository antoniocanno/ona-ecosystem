using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IProxyServerRepository : IBaseRepository<ProxyServer>
    {
        Task<ProxyServer?> GetAvailableProxyAsync();
        Task<ProxyServer?> GetByTenantIdAsync(Guid tenantId);
    }
}

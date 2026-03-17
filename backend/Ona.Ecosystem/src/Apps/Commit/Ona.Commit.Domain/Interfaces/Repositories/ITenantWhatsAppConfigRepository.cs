using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface ITenantWhatsAppConfigRepository : IBaseRepository<TenantWhatsAppConfig>
    {
        Task<TenantWhatsAppConfig> GetOrCreateByTenantIdAsync(Guid tenantId);
        Task<TenantWhatsAppConfig?> GetByTenantIdAsync(Guid tenantId);
        Task<TenantWhatsAppConfig?> GetByInstanceNameAsync(string instanceName);
    }
}

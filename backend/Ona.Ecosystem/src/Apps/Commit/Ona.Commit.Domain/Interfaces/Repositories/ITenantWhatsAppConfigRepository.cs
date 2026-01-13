using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface ITenantWhatsAppConfigRepository : IBaseRepository<TenantWhatsAppConfig>
    {
        Task<TenantWhatsAppConfig?> GetByTenantIdAsync(Guid tenantId);
    }
}

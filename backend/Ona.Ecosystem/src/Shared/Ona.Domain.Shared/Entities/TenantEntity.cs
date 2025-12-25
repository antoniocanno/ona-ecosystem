using Ona.Domain.Shared.Interfaces;

namespace Ona.Domain.Shared.Entities
{
    public abstract class TenantEntity : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; protected set; }
        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }
}

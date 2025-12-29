using Ona.Core.Interfaces;

namespace Ona.Core.Entities
{
    public abstract class TenantEntity : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; protected set; }
        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }
}

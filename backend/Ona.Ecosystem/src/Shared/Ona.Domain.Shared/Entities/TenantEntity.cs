using Ona.Domain.Shared.Interfaces;

namespace Ona.Domain.Shared.Entities
{
    public abstract class TenantEntity : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
    }
}

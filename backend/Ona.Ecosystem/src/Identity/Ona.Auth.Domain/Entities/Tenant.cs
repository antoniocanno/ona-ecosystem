using Ona.Core.Entities;

namespace Ona.Auth.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public TenantStatus Status { get; set; } = TenantStatus.Active;
    }
}

using Microsoft.AspNetCore.Identity;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>, ITenantEntity
    {
        public string Description { get; set; } = string.Empty;
        public Guid TenantId { get; private set; }

        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName, Guid tenantId) : base(roleName)
        {
            NormalizedName = roleName.ToUpperInvariant();
            TenantId = tenantId;
        }

        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }
}

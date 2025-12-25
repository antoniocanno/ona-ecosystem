using Microsoft.AspNetCore.Identity;
using Ona.Core.Common.Enums;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public class UserTenantRole : IdentityUserRole<Guid>, ITenantEntity
    {
        public Guid TenantId { get; private set; }

        public UserTenantRole(Guid userId, Guid roleId, Guid tenantId)
        {
            UserId = userId;
            RoleId = roleId;
            TenantId = tenantId;
        }

        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }
}

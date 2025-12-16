using Microsoft.AspNetCore.Identity;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public class UserTenantRole : IdentityUserRole<Guid>, ITenantEntity
    {
        public Guid TenantId { get; set; }
    }
}

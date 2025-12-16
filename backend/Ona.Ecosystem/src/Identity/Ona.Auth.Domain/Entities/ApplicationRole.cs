using Microsoft.AspNetCore.Identity;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>, ITenantEntity
    {
        public string Description { get; set; } = string.Empty;
        public Guid TenantId { get; set; }

        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}

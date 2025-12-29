using Microsoft.AspNetCore.Identity;
using Ona.Core.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public DateTime? EmailConfirmedAt { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string ColorTheme { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public Guid TenantId { get; set; }

        public List<string?>? Roles { get; set; } = [];

        public void MarkEmailAsVerified()
        {
            EmailConfirmedAt = DateTime.UtcNow;
            EmailConfirmed = true;
            SetUpdatedAt();
        }

        public void Unlock()
        {
            LockoutEnd = null;
            SetUpdatedAt();
        }

        public void Lock(DateTime lockoutEnd)
        {
            LockoutEnd = lockoutEnd;
            SetUpdatedAt();
        }

        public bool HasEmail(string email)
            => Email != null && Email.Equals(email, StringComparison.OrdinalIgnoreCase);

        public void SetLogoUrl(string logoUrl)
        {
            LogoUrl = logoUrl;
            SetUpdatedAt();
        }

        public void SetTenantId(Guid tenantId)
        {
            TenantId = tenantId;
            SetUpdatedAt();
        }

        private void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
    }
}


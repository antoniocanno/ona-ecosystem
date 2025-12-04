using Microsoft.AspNetCore.Identity;

namespace Ona.Auth.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public DateTime? EmailConfirmedAt { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void MarkEmailAsVerified()
        {

            EmailConfirmedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            EmailConfirmed = true;
        }

        public void Unlock()
        {
            LockoutEnd = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Lock(DateTime lockoutEnd)
        {
            LockoutEnd = lockoutEnd;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasEmail(string email)
            => Email != null && Email.Equals(email, StringComparison.OrdinalIgnoreCase);

        public void SetLogoUrl(string logoUrl)
        {
            LogoUrl = logoUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}


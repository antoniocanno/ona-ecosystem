using System.ComponentModel.DataAnnotations.Schema;

namespace Ona.Auth.Domain.Entities
{
    [Table("Users")]
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public bool EmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? LogoUrl { get; set; }

        public void MarkEmailAsVerified()
        {
            EmailVerified = true;
            EmailVerifiedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
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
            => Email.Equals(email, StringComparison.OrdinalIgnoreCase);

        public void SetLogoUrl(string logoUrl)
        {
            LogoUrl = logoUrl;
        }
    }
}

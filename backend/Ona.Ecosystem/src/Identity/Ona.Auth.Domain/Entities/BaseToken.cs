using Ona.Core.Common.Helpers;

namespace Ona.Auth.Domain.Entities
{
    public abstract class BaseToken
    {
        public Guid Id { get; private set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;

        protected BaseToken()
        {
            Id = GuidGenerator.NewSequentialGuid();
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke()
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }
    }
}

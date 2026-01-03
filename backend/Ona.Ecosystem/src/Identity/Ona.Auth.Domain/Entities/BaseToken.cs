using Ona.Core.Common.Exceptions;
using Ona.Core.Common.Helpers;
using Ona.Core.Interfaces;

namespace Ona.Auth.Domain.Entities
{
    public abstract class BaseToken : IUserEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
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

        public void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("O token deve ter um usuário vinculado.");

            UserId = userId;
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

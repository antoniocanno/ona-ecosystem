using Ona.Domain.Shared.Entities;

namespace Ona.Auth.Domain.Entities
{
    public class TenantInvite : TenantEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsConsumed { get; set; }
        public Guid InvitedByUserId { get; set; }

        protected TenantInvite() { }

        public TenantInvite(Guid tenantId, string email, string role, Guid invitedByUserId)
        {
            TenantId = tenantId;
            Email = email;
            Role = role;
            InvitedByUserId = invitedByUserId;
            Token = Guid.NewGuid().ToString("N");
            ExpiresAt = DateTime.UtcNow.AddDays(7);
            IsConsumed = false;
        }
    }
}

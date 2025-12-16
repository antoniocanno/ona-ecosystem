using Ona.Commit.Domain.Enums;
using Ona.Domain.Shared.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class MessageTemplate : TenantEntity
    {
        public NotificationType Type { get; set; }

        public string Content { get; set; } = string.Empty;

        public int OffsetMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

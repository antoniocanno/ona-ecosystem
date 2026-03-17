using Ona.Commit.Domain.Enums;
using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class NotificationLog : TenantEntity
    {
        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public NotificationType Type { get; set; }

        public string ContentSent { get; set; } = string.Empty;

        public NotificationStatus Status { get; set; }

        public string? ExternalMessageId { get; set; }

        public string? ErrorMessage { get; set; }

        public string? HangfireJobId { get; set; }
    }
}

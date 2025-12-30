using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class ExternalCalendarEventMapping : TenantEntity
    {
        public Guid AppointmentId { get; set; }
        public string ExternalEventId { get; set; } = string.Empty;
        public DateTimeOffset LastSyncedAt { get; set; }
        public string? ETag { get; set; }

        public virtual Appointment? Appointment { get; set; }

        public ExternalCalendarEventMapping() { }
    }
}

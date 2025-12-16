using Ona.Commit.Domain.Enums;
using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Domain.Entities
{
    public class Appointment : TenantEntity, IUserEntity
    {
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? ExternalCalendarEventId { get; set; }

        public ICollection<NotificationLog> Notifications { get; set; }
    }
}

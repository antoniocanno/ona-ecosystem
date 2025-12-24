using Ona.Commit.Domain.Enums;

namespace Ona.Commit.Application.DTOs.Request
{
    public record CreateAppointmentRequest
    {
        public Guid CustomerId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}

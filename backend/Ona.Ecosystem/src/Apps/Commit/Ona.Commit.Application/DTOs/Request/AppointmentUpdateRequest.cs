using Ona.Commit.Domain.Enums;

namespace Ona.Commit.Application.DTOs.Request
{
    public record AppointmentUpdateRequest
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
}

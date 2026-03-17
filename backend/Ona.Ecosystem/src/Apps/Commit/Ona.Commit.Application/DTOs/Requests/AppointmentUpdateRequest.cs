using Ona.Commit.Domain.Enums;

namespace Ona.Commit.Application.DTOs.Requests
{
    public record AppointmentUpdateRequest
    {
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
}

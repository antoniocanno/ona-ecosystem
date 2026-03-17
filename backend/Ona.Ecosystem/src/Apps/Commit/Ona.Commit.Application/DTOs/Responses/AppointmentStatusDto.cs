using Ona.Commit.Domain.Enums;

namespace Ona.Commit.Application.DTOs.Responses
{
    public record AppointmentStatusDto
    {
        public AppointmentStatus Status { get; set; }
    }
}

using Mapster;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;

namespace Ona.Commit.Application.DTOs.Responses
{
    public record AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public AppointmentStatus Status { get; set; }

        public static implicit operator AppointmentDto(Appointment appointment)
            => appointment.Adapt<AppointmentDto>();
    }
}

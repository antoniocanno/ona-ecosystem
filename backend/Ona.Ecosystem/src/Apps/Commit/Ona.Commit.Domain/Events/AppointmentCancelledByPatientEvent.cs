using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Events
{
    public class AppointmentCancelledByPatientEvent : IDomainEvent
    {
        public Appointment Appointment { get; }
        public DateTimeOffset OccurredOn { get; }

        public AppointmentCancelledByPatientEvent(Appointment appointment)
        {
            Appointment = appointment;
            OccurredOn = DateTimeOffset.UtcNow;
        }
    }
}

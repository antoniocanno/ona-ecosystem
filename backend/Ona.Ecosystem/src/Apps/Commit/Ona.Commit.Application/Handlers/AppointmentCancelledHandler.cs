using Hangfire;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Events;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Handlers
{
    public class AppointmentCancelledHandler : IDomainEventHandler<AppointmentCancelledByPatientEvent>
    {
        public Task HandleAsync(AppointmentCancelledByPatientEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var appointment = domainEvent.Appointment;

            BackgroundJob.Enqueue<ICalendarService>(x => x.DeleteAppointmentEventAsync(appointment));
            BackgroundJob.Enqueue<INotificationService>(x => x.SendCancellationAckAsync(appointment.Id));
            BackgroundJob.Enqueue<INotificationService>(x => x.NotifyProfessionalCancellationAsync(appointment.Id));
            BackgroundJob.Enqueue<INotificationService>(x => x.NotifyOperatorOnCancellationAsync(appointment.Id));

            return Task.CompletedTask;
        }
    }
}

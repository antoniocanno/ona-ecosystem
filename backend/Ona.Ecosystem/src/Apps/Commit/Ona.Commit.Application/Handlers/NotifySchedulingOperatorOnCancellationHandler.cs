using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Events;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Handlers
{
    public class NotifySchedulingOperatorOnCancellationHandler : IDomainEventHandler<AppointmentCancelledByPatientEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IProfessionalAppService _professionalAppService;

        public NotifySchedulingOperatorOnCancellationHandler(
            INotificationService notificationService,
            IProfessionalAppService professionalAppService)
        {
            _notificationService = notificationService;
            _professionalAppService = professionalAppService;
        }

        public async Task HandleAsync(AppointmentCancelledByPatientEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var appointment = domainEvent.Appointment;

            if (appointment.StartDate > DateTimeOffset.UtcNow)
            {
                var professional = await _professionalAppService.GetByIdAsync(appointment.ProfessionalId);
                if (professional == null) return;

                var localTime = appointment.StartDate.ToLocalTime();

                var message = $"O horário das {localTime:HH:mm} do dia {localTime:dd/MM} ficou livre (Agenda de {professional.Name}). Tente encaixar alguém da lista de espera.";

                await _notificationService.NotifyOperatorAsync(message, "Horário Disponível");
            }
        }
    }
}

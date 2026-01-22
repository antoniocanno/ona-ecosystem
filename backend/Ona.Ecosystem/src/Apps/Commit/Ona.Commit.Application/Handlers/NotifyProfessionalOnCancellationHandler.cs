using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Events;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Handlers
{
    public class NotifyProfessionalOnCancellationHandler : IDomainEventHandler<AppointmentCancelledByPatientEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IProfessionalRepository _professionalRepository;

        public NotifyProfessionalOnCancellationHandler(
            INotificationService notificationService,
            IProfessionalRepository professionalRepository)
        {
            _notificationService = notificationService;
            _professionalRepository = professionalRepository;
        }

        public async Task HandleAsync(AppointmentCancelledByPatientEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var appointment = domainEvent.Appointment;
            var professional = await _professionalRepository.GetByIdAsync(appointment.ProfessionalId);

            if (professional == null) return;

            var message = $"Prezado(a) {professional.Name}. O paciente do horário {appointment.StartDate:dd/MM/yyyy HH:mm} cancelou o agendamento.";

            await _notificationService.NotifyProfessionalAsync(professional, message, "Cancelamento de Agenda");
        }
    }
}

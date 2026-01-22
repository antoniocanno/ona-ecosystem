using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IWhatsAppAppService _whatsAppService;
        private readonly IOperatorAlertRepository _alertRepository;
        private readonly ICurrentTenant _currentTenant;
        private readonly ILogger<NotificationService> _logger;
        private readonly IAppointmentRepository _appointmentRepository;
        // private readonly IEmailService _emailService;

        public NotificationService(
            IWhatsAppAppService whatsAppService,
            IOperatorAlertRepository alertRepository,
            IAppointmentRepository appointmentRepository,
            ICurrentTenant currentTenant,
            ILogger<NotificationService> logger)
        {
            _whatsAppService = whatsAppService;
            _alertRepository = alertRepository;
            _appointmentRepository = appointmentRepository;
            _currentTenant = currentTenant;
            _logger = logger;
        }

        private async Task NotifyProfessionalAsync(Professional professional, string message, string subject = "Notificação do Sistema")
        {
            if (_currentTenant.Id == null) return;

            // Priority 1: WhatsApp
            if (!string.IsNullOrEmpty(professional.PhoneNumber))
            {
                try
                {
                    await _whatsAppService.SendTextMessageAsync(_currentTenant.Id.Value, professional.PhoneNumber, message);
                }
                catch
                {
                    _logger.LogError("Falha ao enviar mensagem para profissional {ProfessionalName} no número {PhoneNumber}", professional.Name, professional.PhoneNumber);
                }
            }

            // Priority 2: Email (TODO)
            // if (!string.IsNullOrEmpty(professional.Email)) { ... }
        }

        private async Task SetOperatorAlertAsync(string message, string title = "Alerta do Sistema")
        {
            var alert = new OperatorAlert(title, message);
            await _alertRepository.CreateAsync(alert);
            await _alertRepository.SaveChangesAsync();
        }

        public async Task SendCancellationAckAsync(Guid appointmentId)
        {

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, a => a.Customer!, a => a.Professional!);
            if (appointment == null || appointment.TenantId == Guid.Empty) return;

            if (appointment.Customer != null && !string.IsNullOrEmpty(appointment.Customer.PhoneNumber))
            {
                var message = $"Seu agendamento com {appointment.Professional?.Name} para {appointment.StartDate:dd/MM HH:mm} foi cancelado com sucesso. Caso queira reagendar, entre em contato.";
                try
                {
                    await _whatsAppService.SendTextMessageAsync(appointment.TenantId, appointment.Customer.PhoneNumber, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao enviar mensagem de cancelamento para {PhoneNumber}", appointment.Customer.PhoneNumber);
                }
            }
        }

        public async Task NotifyProfessionalCancellationAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, a => a.Professional!);
            if (appointment == null || appointment.Professional == null) return;

            var professional = appointment.Professional;
            var message = $"Prezado(a) {professional.Name}. O paciente do horário {appointment.StartDate:dd/MM/yyyy HH:mm} cancelou o agendamento.";

            await NotifyProfessionalAsync(professional, message, "Cancelamento de Agenda");
        }

        public async Task NotifyOperatorOnCancellationAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, a => a.Professional!);
            if (appointment == null || appointment.Professional == null) return;

            if (appointment.StartDate > DateTimeOffset.UtcNow)
            {
                var localTime = appointment.StartDate.ToLocalTime();
                var message = $"O horário das {localTime:HH:mm} do dia {localTime:dd/MM} ficou livre (Agenda de {appointment.Professional.Name}). Tente encaixar alguém da lista de espera.";

                await SetOperatorAlertAsync(message, "Horário Disponível");
            }
        }
    }
}

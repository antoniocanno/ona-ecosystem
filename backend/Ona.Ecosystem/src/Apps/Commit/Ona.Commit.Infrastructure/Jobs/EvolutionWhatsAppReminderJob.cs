using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Gateways;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class EvolutionWhatsAppReminderJob : IWhatsAppReminderJob
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITemplateMessageBuilder _messageBuilder;
        private readonly IWhatsAppGateway _whatsAppGateway;
        private readonly IMessageInteractionLogRepository _logRepository;
        private readonly ILogger<EvolutionWhatsAppReminderJob> _logger;

        public EvolutionWhatsAppReminderJob(
            IAppointmentRepository appointmentRepository,
            ITemplateMessageBuilder messageBuilder,
            IWhatsAppGateway whatsAppGateway,
            IMessageInteractionLogRepository logRepository,
            ILogger<EvolutionWhatsAppReminderJob> logger)
        {
            _appointmentRepository = appointmentRepository;
            _messageBuilder = messageBuilder;
            _whatsAppGateway = whatsAppGateway;
            _logRepository = logRepository;
            _logger = logger;
        }

        public async Task ProcessAndSendReminderAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, a => a.Customer!);

            if (appointment == null)
            {
                _logger.LogWarning("Job abortado: Agendamento {Id} não encontrado.", appointmentId);
                return;
            }

            _logger.LogInformation("Processando lembrete para Agendamento {Id}...", appointmentId);

            try
            {
                var message = await _messageBuilder.BuildTextReminderAsync(appointment);

                if (!string.IsNullOrEmpty(appointment.Customer?.PhoneNumber))
                {
                    var instanceName = $"tenant_{appointment.TenantId:N}";

                    var externalMessageId = await _whatsAppGateway.SendTextMessageAsync(instanceName, appointment.Customer!.PhoneNumber, message);

                    var log = new MessageInteractionLog(
                        appointment.TenantId,
                        externalMessageId,
                        appointment.Id,
                        0.0m,
                        NotificationStatus.Sent,
                        message
                    );

                    await _logRepository.CreateAsync(log);

                    appointment.MarkReminderAsSent();
                    _appointmentRepository.Update(appointment);
                    await _appointmentRepository.SaveChangesAsync();

                    _logger.LogInformation("Lembrete enviado com sucesso via Evolution. ID: {ExternalId}", externalMessageId);

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar lembrete via Evolution para Agendamento {Id}.", appointmentId);
            }

            // TODO: Implementar fallback para SMS
            //if (!string.IsNullOrEmpty(appointment.Customer?.PhoneNumber)) { ... }

            // TODO: Implementar fallback para Email
            //if (!string.IsNullOrEmpty(appointment.Customer?.Email)) { ... }
        }
    }
}

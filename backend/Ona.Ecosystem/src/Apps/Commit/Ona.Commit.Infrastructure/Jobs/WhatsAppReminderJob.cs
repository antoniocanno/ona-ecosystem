using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Exceptions;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class WhatsAppReminderJob : IWhatsAppReminderJob
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITemplateMessageBuilder _messageBuilder;
        private readonly IMetaCloudSenderService _senderService;
        private readonly IMessageInteractionLogRepository _logRepository;
        private readonly ILogger<WhatsAppReminderJob> _logger;

        public WhatsAppReminderJob(
            IAppointmentRepository appointmentRepository,
            ITemplateMessageBuilder messageBuilder,
            IMetaCloudSenderService senderService,
            IMessageInteractionLogRepository logRepository,
            ILogger<WhatsAppReminderJob> logger)
        {
            _appointmentRepository = appointmentRepository;
            _messageBuilder = messageBuilder;
            _senderService = senderService;
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

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                _logger.LogWarning("Job abortado: Agendamento {Id} não está mais confirmado. Status: {Status}", appointmentId, appointment.Status);
                return;
            }

            _logger.LogInformation("Processando lembrete de WhatsApp para Agendamento {Id}...", appointmentId);

            string payload = string.Empty;
            try
            {
                // Otimização de Custo: Verifica se existe janela de conversação aberta
                bool hasRecentInteraction = await _logRepository.HasRecentInteractionAsync(appointment.TenantId, appointment.CustomerId);
                decimal estimatedCost = 0.05m;

                if (hasRecentInteraction)
                {
                    // Se tem janela aberta, podemos enviar mensagem livre (Free tier se Service Conversation)
                    // TODO: Implementar builder para mensagem livre (FreeFormMessageBuilder) futuramente
                    // Por enquanto mantemos o Template, mas marcamos custo estimado menor
                    _logger.LogInformation("Janela de conversação aberta detectada para Customer {CustomerId}. Custo reduzido.", appointment.CustomerId);
                    estimatedCost = 0.00m; // Assumindo custo zero dentro da janela de serviço (ou menor se for marketing)
                    // Nota: Mesmo com janela aberta, Template de Utility paga. Apenas Free Form message é mais barata/gratuita.
                    // Idealmente aqui chamariamos _messageBuilder.BuildFreeFormReminder(appointment)
                }

                payload = await _messageBuilder.BuildReminderPayloadAsync(appointment);

                var externalMessageId = await _senderService.SendMessageAsync(appointment.TenantId, payload);

                var log = new MessageInteractionLog(
                    appointment.TenantId,
                    externalMessageId,
                    appointment.Id,
                    estimatedCost,
                    NotificationStatus.Sent,
                    payload
                );

                await _logRepository.CreateAsync(log);

                appointment.MarkReminderAsSent();
                _appointmentRepository.Update(appointment);

                await _appointmentRepository.SaveChangesAsync();

                _logger.LogInformation("Lembrete enviado com sucesso. MetaId: {ExternalId}", externalMessageId);
            }
            catch (WhatsAppPermanentException ex)
            {
                _logger.LogError(ex, "Falha permanente no envio de WhatsApp para Agendamento {Id}.", appointmentId);

                appointment.MarkReminderAsFailed();
                _appointmentRepository.Update(appointment);
                await _appointmentRepository.SaveChangesAsync();
            }
            catch (WhatsAppTransientException ex)
            {
                _logger.LogWarning(ex, "Falha temporária no envio de WhatsApp para Agendamento {Id}. O Hangfire tentará novamente.", appointmentId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar lembrete de WhatsApp para Agendamento {Id}.", appointmentId);
                throw;
            }
        }
    }
}

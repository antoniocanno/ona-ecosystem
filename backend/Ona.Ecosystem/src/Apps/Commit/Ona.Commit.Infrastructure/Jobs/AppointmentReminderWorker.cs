using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Workers;
using Ona.Commit.Infrastructure.Data;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class AppointmentReminderWorker : IAppointmentReminderWorker
    {
        private readonly CommitDbContext _context;
        private readonly ILogger<AppointmentReminderWorker> _logger;

        public AppointmentReminderWorker(
            CommitDbContext context,
            ILogger<AppointmentReminderWorker> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendPendingRemindersAsync()
        {
            _logger.LogInformation("Iniciando verificação de lembretes de agendamentos pendentes...");

            try
            {
                var now = DateTimeOffset.UtcNow;

                var appointmentsToRemind = await _context.Appointments
                    .Where(a =>
                        a.ReminderLeadTime.HasValue &&
                        a.ReminderStatus == ReminderStatus.Pending &&
                        a.Status == AppointmentStatus.Pending &&
                        a.StartDate.AddMinutes(-a.ReminderLeadTime.Value) <= now &&
                        a.StartDate > now)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} agendamentos para enviar lembretes.", appointmentsToRemind.Count);

                int sentCount = 0;
                int failedCount = 0;

                foreach (var appointment in appointmentsToRemind)
                {
                    try
                    {
                        _logger.LogInformation(
                            "LEMBRETE ENVIADO | AppointmentId: {AppointmentId} | CustomerId: {CustomerId} | ProfessionalId: {ProfessionalId} | StartDate: {StartDate} | LeadTime: {LeadTime} minutos",
                            appointment.Id,
                            appointment.CustomerId,
                            appointment.ProfessionalId,
                            appointment.StartDate,
                            appointment.ReminderLeadTime);

                        appointment.MarkReminderAsSent();

                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Erro ao enviar lembrete para o agendamento {AppointmentId}",
                            appointment.Id);

                        appointment.MarkReminderAsFailed();

                        failedCount++;
                    }
                }

                if (sentCount > 0 || failedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        "Processamento concluído: {SentCount} lembretes enviados, {FailedCount} falharam.",
                        sentCount,
                        failedCount);
                }
                else
                {
                    _logger.LogInformation("Nenhum lembrete foi processado.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao processar lembretes de agendamentos.");
                throw;
            }

            _logger.LogInformation("Verificação de lembretes finalizada.");
        }
    }
}

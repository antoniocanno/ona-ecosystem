using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Workers;
using Ona.Commit.Infrastructure.Data;
using Ona.Core.Tenant;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class AppointmentReminderScheduler : IAppointmentReminderScheduler
    {
        private readonly CommitDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<AppointmentReminderScheduler> _logger;

        public AppointmentReminderScheduler(
            CommitDbContext context,
            IBackgroundJobClient backgroundJobClient,
            ITenantProvider tenantProvider,
            ILogger<AppointmentReminderScheduler> logger)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task ScheduleRemindersAsync()
        {
            var now = DateTimeOffset.UtcNow;

            _logger.LogInformation("Iniciando agendamento de lembretes de consultas...");

            var minSearchDate = new DateTimeOffset(now.Date, TimeSpan.Zero);
            var maxSearchDate = minSearchDate.AddDays(3);

            var allPendingAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Pending &&
                            a.ReminderStatus == ReminderStatus.Pending &&
                            a.StartDate >= minSearchDate &&
                            a.StartDate <= maxSearchDate)
                .ToListAsync();

            if (allPendingAppointments.Count == 0)
            {
                _logger.LogInformation("Nenhum agendamento confirmado pendente encontrado para processamento.");
                return;
            }

            var appointmentsByTenant = allPendingAppointments.GroupBy(a => a.TenantId);
            int totalEnqueued = 0;

            foreach (var tenantGroup in appointmentsByTenant)
            {
                var tenantId = tenantGroup.Key;
                var tenantContext = await _tenantProvider.GetAsync(tenantId);

                if (tenantContext == null)
                {
                    _logger.LogWarning("Tenant {TenantId} não encontrado ao processar lembretes.", tenantId);
                    continue;
                }

                var timeZone = GetTimeZone(tenantContext.TimeZone);
                var nowInTenantZone = TimeZoneInfo.ConvertTime(now, timeZone);

                if (nowInTenantZone.Hour < 8 || nowInTenantZone.Hour >= 20)
                {
                    _logger.LogDebug("Tenant {TenantName} ({TenantId}) fora do horário comercial ({Hour}h). Ignorado nesta execução.",
                        tenantContext.Name, tenantId, nowInTenantZone.Hour);
                    continue;
                }

                var tomorrowDateInTenantZone = nowInTenantZone.Date.AddDays(1);
                var tomorrowStartInTenantZone = new DateTimeOffset(tomorrowDateInTenantZone, timeZone.GetUtcOffset(tomorrowDateInTenantZone));
                var tomorrowEndInTenantZone = tomorrowStartInTenantZone.AddDays(1);

                var tenantAppointmentsToRemind = tenantGroup
                    .Where(a => a.StartDate >= tomorrowStartInTenantZone && a.StartDate < tomorrowEndInTenantZone)
                    .ToList();

                if (tenantAppointmentsToRemind.Count == 0) continue;

                _logger.LogInformation("Enfileirando {Count} lembretes para o Tenant {TenantName} ({TenantId}). Local Time: {LocalTime}",
                    tenantAppointmentsToRemind.Count, tenantContext.Name, tenantId, nowInTenantZone);

                foreach (var appointment in tenantAppointmentsToRemind)
                {
                    _backgroundJobClient.Enqueue<IWhatsAppReminderJob>(job => job.ProcessAndSendReminderAsync(appointment.Id));
                    appointment.MarkReminderAsScheduled();
                    totalEnqueued++;
                }
            }

            if (totalEnqueued > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Processo de agendamento concluído. {Total} lembretes enfileirados.", totalEnqueued);
            }
            else
            {
                _logger.LogInformation("Processo de agendamento concluído. Nenhum lembrete enfileirado no momento.");
            }
        }

        private TimeZoneInfo GetTimeZone(string? timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
                return TimeZoneInfo.Utc;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception)
            {
                _logger.LogWarning("Fuso horário '{TimeZoneId}' não reconhecido. Usando UTC.", timeZoneId);
                return TimeZoneInfo.Utc;
            }
        }
    }
}

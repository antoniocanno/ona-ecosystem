using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Workers;
using Ona.Commit.Infrastructure.Data;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class DailyReminderScheduler : IDailyReminderScheduler
    {
        private readonly CommitDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DailyReminderScheduler> _logger;

        public DailyReminderScheduler(
            CommitDbContext context,
            IBackgroundJobClient backgroundJobClient,
            ILogger<DailyReminderScheduler> logger)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task ScheduleDailyRemindersAsync()
        {
            var now = DateTimeOffset.UtcNow;

            // TODO: Ajustar para o fuso horário do Tenant se necessário no futuro
            if (now.Hour < 8 || now.Hour >= 20)
            {
                _logger.LogInformation("Fora do horário de atividade (08h-20h). Scheduler ignorado.");
                return;
            }

            _logger.LogInformation("Iniciando agendamento de lembretes diários para amanhã...");

            var tomorrowStart = now.Date.AddDays(1);
            var tomorrowEnd = tomorrowStart.AddDays(1).AddTicks(-1);

            var appointmentsToRemind = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.Status == AppointmentStatus.Confirmed &&
                            a.ReminderStatus == ReminderStatus.Pending &&
                            a.StartDate >= tomorrowStart &&
                            a.StartDate <= tomorrowEnd)
                .ToListAsync();

            if (!appointmentsToRemind.Any())
            {
                _logger.LogInformation("Nenhum agendamento confirmado encontrado para amanhã ({Date}).", tomorrowStart.ToShortDateString());
                return;
            }

            _logger.LogInformation("Enfileirando {Count} lembretes individuais.", appointmentsToRemind.Count);

            foreach (var appointment in appointmentsToRemind)
            {
                _backgroundJobClient.Enqueue<IWhatsAppReminderJob>(job => job.ProcessAndSendReminderAsync(appointment.Id));
                appointment.MarkReminderAsScheduled();
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Processo de agendamento concluído.");
        }
    }
}

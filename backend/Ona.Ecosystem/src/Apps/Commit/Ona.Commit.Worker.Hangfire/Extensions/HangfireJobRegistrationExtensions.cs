using Hangfire;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Worker.Hangfire.Extensions;

public static class HangfireJobRegistrationExtensions
{
    public static IHost RegisterHangfireJobs(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<ICalendarTokenRefreshWorker>(
            "calendar-token-refresh",
            worker => worker.RefreshExpiringTokensAsync(),
            Cron.MinuteInterval(5));

        recurringJobManager.AddOrUpdate<IAppointmentReminderScheduler>(
            "whatsapp-appointment-reminder-scheduler",
            scheduler => scheduler.ScheduleRemindersAsync(),
            Cron.HourInterval(1));

        return host;
    }
}

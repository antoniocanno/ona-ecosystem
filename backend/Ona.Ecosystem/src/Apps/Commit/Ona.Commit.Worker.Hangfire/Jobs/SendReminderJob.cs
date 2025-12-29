namespace Ona.Commit.Worker.Hangfire.Jobs;

public class SendReminderJob
{
    private readonly ILogger<SendReminderJob> _logger;

    public SendReminderJob(ILogger<SendReminderJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(Guid appointmentId)
    {
        _logger.LogInformation("Sending reminder for appointment {AppointmentId}...", appointmentId);

        // Simulação de processamento
        await Task.Delay(1000);

        _logger.LogInformation("Reminder sent successfully for appointment {AppointmentId}.", appointmentId);
    }
}

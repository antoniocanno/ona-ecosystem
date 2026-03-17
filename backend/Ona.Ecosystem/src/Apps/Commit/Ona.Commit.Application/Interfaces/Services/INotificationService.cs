namespace Ona.Commit.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendCancellationAckAsync(Guid appointmentId);
        Task NotifyProfessionalCancellationAsync(Guid appointmentId);
        Task NotifyOperatorOnCancellationAsync(Guid appointmentId);
    }
}

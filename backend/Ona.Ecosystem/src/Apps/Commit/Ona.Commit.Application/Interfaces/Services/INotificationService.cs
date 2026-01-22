using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyProfessionalAsync(Professional professional, string message, string subject = "Notificação do Sistema");
        Task NotifyOperatorAsync(string message, string title = "Alerta do Sistema");
    }
}

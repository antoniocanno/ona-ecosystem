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
        // private readonly IEmailService _emailService;

        public NotificationService(
            IWhatsAppAppService whatsAppService,
            IOperatorAlertRepository alertRepository,
            ICurrentTenant currentTenant,
            ILogger<NotificationService> logger)
        {
            _whatsAppService = whatsAppService;
            _alertRepository = alertRepository;
            _currentTenant = currentTenant;
            _logger = logger;
        }

        public async Task NotifyProfessionalAsync(Professional professional, string message, string subject = "Notificação do Sistema")
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

        public async Task NotifyOperatorAsync(string message, string title = "Alerta do Sistema")
        {
            var alert = new OperatorAlert(title, message);
            await _alertRepository.CreateAsync(alert);
            await _alertRepository.SaveChangesAsync();
        }
    }
}

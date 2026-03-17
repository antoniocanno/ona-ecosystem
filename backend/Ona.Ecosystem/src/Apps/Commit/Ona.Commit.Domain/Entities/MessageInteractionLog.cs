using Ona.Commit.Domain.Enums;
using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    /// <summary>
    /// Tabela de auditoria crítica para interações de mensagens via WhatsApp.
    /// </summary>
    public class MessageInteractionLog : TenantEntity
    {
        public string ExternalMessageId { get; set; } = string.Empty;

        public Guid AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public decimal EstimatedCost { get; set; }

        public NotificationStatus Status { get; set; }

        public string Payload { get; set; } = string.Empty;

        public MessageInteractionLog()
        {
        }

        public MessageInteractionLog(
            Guid tenantId,
            string externalMessageId,
            Guid appointmentId,
            decimal estimatedCost,
            NotificationStatus status,
            string payload)
        {
            SetTenantId(tenantId);
            ExternalMessageId = externalMessageId;
            AppointmentId = appointmentId;
            EstimatedCost = estimatedCost;
            Status = status;
            Payload = payload;
        }

        public void UpdateStatus(NotificationStatus status)
        {
            Status = status;
            Update();
        }
    }
}

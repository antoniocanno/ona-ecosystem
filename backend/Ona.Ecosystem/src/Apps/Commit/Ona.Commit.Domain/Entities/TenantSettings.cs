using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class TenantSettings : TenantEntity
    {
        public string BusinessName { get; set; } = string.Empty;

        public string TimezoneId { get; set; } = "America/Sao_Paulo";

        public string? WhatsAppInstanceId { get; set; }
        public string? WhatsAppApiKey { get; set; }
        public bool IsWhatsAppConnected { get; set; } = false;

        public ICollection<MessageTemplate> Templates { get; set; } = [];
    }
}

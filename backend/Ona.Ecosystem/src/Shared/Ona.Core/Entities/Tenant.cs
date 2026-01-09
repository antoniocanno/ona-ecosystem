using Ona.Core.Common.Enums;

namespace Ona.Core.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "America/Sao_Paulo";
        public TenantStatus Status { get; set; } = TenantStatus.Active;

        public string? WhatsAppInstanceId { get; set; }
        public string? WhatsAppApiKey { get; set; }
        public bool IsWhatsAppConnected { get; set; } = false;
    }
}

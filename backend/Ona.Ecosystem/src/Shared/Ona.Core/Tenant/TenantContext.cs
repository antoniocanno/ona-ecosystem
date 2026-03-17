namespace Ona.Core.Tenant
{
    public class TenantContext
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public string? Domain { get; set; }
        public string TimeZone { get; set; }

        public string? WhatsAppApiKey { get; set; }
    }
}

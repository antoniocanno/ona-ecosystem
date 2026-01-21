using Ona.Commit.Domain.Enums;
using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class TenantWhatsAppConfig : TenantEntity
    {
        public WhatsAppProvider Provider { get; set; } = WhatsAppProvider.None;
        public bool IsUsingSharedAccount { get; set; } = true;

        // Meta Cloud API
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }

        // Evolution API
        public string? InstanceName { get; set; }
        public string? ApiKey { get; set; }

        // Infrastructure
        public Guid? ProxyServerId { get; set; }
        public ProxyServer? ProxyServer { get; set; }

        public TenantWhatsAppConfig()
        {
        }

        public TenantWhatsAppConfig(Guid tenantId, WhatsAppProvider provider, bool isUsingSharedAccount = true)
        {
            SetTenantId(tenantId);
            Provider = provider;
            IsUsingSharedAccount = isUsingSharedAccount;
        }

        public void UpdateMetaCredentials(string? phoneNumberId, string? wabaId)
        {
            Provider = WhatsAppProvider.MetaCloud;
            PhoneNumberId = phoneNumberId;
            WabaId = wabaId;
            IsUsingSharedAccount = string.IsNullOrEmpty(phoneNumberId) && string.IsNullOrEmpty(wabaId);
            Update();
        }

        public void UpdateEvolutionCredentials(string instanceName, string? apiKey)
        {
            Provider = WhatsAppProvider.Evolution;
            InstanceName = instanceName;
            ApiKey = apiKey;
            IsUsingSharedAccount = false;
            Update();
        }

        public void UseSharedAccount()
        {
            IsUsingSharedAccount = true;
            Provider = WhatsAppProvider.None;
            PhoneNumberId = null;
            WabaId = null;
            InstanceName = null;
            ApiKey = null;
            Update();
        }
    }
}

using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class TenantWhatsAppConfig : TenantEntity
    {
        public bool IsUsingSharedAccount { get; set; } = true;
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }

        public TenantWhatsAppConfig()
        {
        }

        public TenantWhatsAppConfig(Guid tenantId, bool isUsingSharedAccount = true)
        {
            SetTenantId(tenantId);
            IsUsingSharedAccount = isUsingSharedAccount;
        }

        public void UpdateCredentials(string? phoneNumberId, string? wabaId)
        {
            PhoneNumberId = phoneNumberId;
            WabaId = wabaId;
            IsUsingSharedAccount = string.IsNullOrEmpty(phoneNumberId) && string.IsNullOrEmpty(wabaId);
            Update();
        }

        public void UseSharedAccount()
        {
            IsUsingSharedAccount = true;
            PhoneNumberId = null;
            WabaId = null;
            Update();
        }
    }
}

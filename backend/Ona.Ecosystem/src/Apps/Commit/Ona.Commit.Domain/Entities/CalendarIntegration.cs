using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class CalendarIntegration : TenantEntity
    {
        public Guid CustomerId { get; set; }
        public CalendarProvider Provider { get; set; }
        public string EncryptedRefreshToken { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public DateTime TokenIssuedAtUtc { get; set; } = DateTime.UtcNow;
        public string? ExternalEmailAddress { get; set; }
        public string? ExternalCalendarId { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;

        // Webhook / Sync fields
        public string? ExternalResourceId { get; set; }
        public string? ExternalChannelId { get; set; }
        public string? SyncToken { get; set; }
        public string? NextSyncToken { get; set; }

        public CalendarIntegration()
        {
        }
    }

    public enum CalendarProvider
    {
        Google,
        Outlook
    }
}

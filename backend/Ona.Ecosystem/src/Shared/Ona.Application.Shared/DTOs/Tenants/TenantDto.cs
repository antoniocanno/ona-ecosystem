using Ona.Core.Common.Enums;

namespace Ona.Application.Shared.DTOs.Tenants
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public TenantStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}

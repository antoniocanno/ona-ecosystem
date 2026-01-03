using System.ComponentModel.DataAnnotations;

namespace Ona.Application.Shared.DTOs.Tenants
{
    public class UpdateTenantRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Domain { get; set; }

        [MaxLength(100)]
        public string? TimeZone { get; set; }
    }
}

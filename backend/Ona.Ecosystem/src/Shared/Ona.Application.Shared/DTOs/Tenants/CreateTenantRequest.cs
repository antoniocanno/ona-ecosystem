namespace Ona.Application.Shared.DTOs.Tenants
{
    public record CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "America/Sao_Paulo";
    }
}

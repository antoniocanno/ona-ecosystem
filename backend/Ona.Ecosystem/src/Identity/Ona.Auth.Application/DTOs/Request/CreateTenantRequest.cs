namespace Ona.Auth.Application.DTOs.Request
{
    public record CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }
}

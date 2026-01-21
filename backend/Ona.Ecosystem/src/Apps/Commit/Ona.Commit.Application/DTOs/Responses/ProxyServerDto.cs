
using Mapster;
using Ona.Commit.Domain.Entities;
namespace Ona.Commit.Application.DTOs.Responses
{
    public class ProxyServerDto
    {
        public Guid Id { get; set; }
        public string Host { get; set; } = null!;
        public string Port { get; set; } = null!;
        public string Protocol { get; set; } = null!;
        public string? Username { get; set; }
        public int MaxTenants { get; set; }
        public bool IsActive { get; set; }
        public int CurrentTenantsCount { get; set; }

        public static implicit operator ProxyServerDto(ProxyServer proxy)
            => proxy.Adapt<ProxyServerDto>();
    }
}

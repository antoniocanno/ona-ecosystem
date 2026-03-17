using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class ProxyServer : BaseEntity
    {
        public string Host { get; private set; } = null!;
        public string Port { get; private set; } = null!;
        public string Protocol { get; private set; } = "http"; // http, socks5
        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public int MaxTenants { get; private set; }
        public bool IsActive { get; private set; }

        public ICollection<TenantWhatsAppConfig> Tenants { get; private set; } = [];

        protected ProxyServer() { }

        public ProxyServer(string host, string port, string protocol, int maxTenants, string? username = null, string? password = null)
        {
            Host = host;
            Port = port;
            Protocol = protocol;
            MaxTenants = maxTenants;
            Username = username;
            Password = password;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
            Update();
        }

        public void Activate()
        {
            IsActive = true;
            Update();
        }

        public bool CanAcceptNewTenant()
        {
            return IsActive && Tenants.Count < MaxTenants;
        }
    }
}

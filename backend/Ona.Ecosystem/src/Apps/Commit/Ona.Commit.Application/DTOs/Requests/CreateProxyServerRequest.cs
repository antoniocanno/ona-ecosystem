using System.ComponentModel.DataAnnotations;

namespace Ona.Commit.Application.DTOs.Requests
{
    public class CreateProxyServerRequest
    {
        [Required]
        public string Host { get; set; } = null!;

        [Required]
        public string Port { get; set; } = null!;

        [Required]
        public string Protocol { get; set; } = "http"; // http, socks5

        public string? Username { get; set; }

        public string? Password { get; set; }

        [Range(1, 1000)]
        public int MaxTenants { get; set; } = 10;
    }
}

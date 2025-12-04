using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Request
{
    public record LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Request
{
    public record ResendVerificationEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

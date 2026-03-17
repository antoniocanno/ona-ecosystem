using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Requests
{
    public record ResendVerificationEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

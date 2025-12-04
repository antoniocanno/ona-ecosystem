using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Request
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Requests
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

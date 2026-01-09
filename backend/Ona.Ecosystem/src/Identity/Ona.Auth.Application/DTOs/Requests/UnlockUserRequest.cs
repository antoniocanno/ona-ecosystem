using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Requests
{
    public record UnlockUserRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

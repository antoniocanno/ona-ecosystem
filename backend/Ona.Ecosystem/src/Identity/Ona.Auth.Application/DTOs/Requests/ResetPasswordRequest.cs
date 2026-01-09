using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}

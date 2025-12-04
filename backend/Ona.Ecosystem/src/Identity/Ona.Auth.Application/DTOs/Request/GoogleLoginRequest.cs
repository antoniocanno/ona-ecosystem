using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Request
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}

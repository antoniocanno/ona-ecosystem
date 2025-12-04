using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Request
{
    public record UserUpdateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ona.Auth.Application.DTOs.Requests
{
    public record UserUpdateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}

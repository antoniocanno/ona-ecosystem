namespace Ona.Auth.Application.DTOs.Responses
{
    public record UserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

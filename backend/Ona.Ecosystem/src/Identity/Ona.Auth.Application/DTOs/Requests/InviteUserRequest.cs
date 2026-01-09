namespace Ona.Auth.Application.DTOs.Requests
{
    public class InviteUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

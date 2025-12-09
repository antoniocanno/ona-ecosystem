namespace Ona.Auth.Application.Models
{
    public class PasswordResetModel
    {
        public string? UserName { get; set; }
        public string? PasswordResetUrl { get; set; }
        public int ExpirationHours { get; set; }
        public string? SupportEmail { get; set; }
        public string? CompanyName { get; set; }
    }
}

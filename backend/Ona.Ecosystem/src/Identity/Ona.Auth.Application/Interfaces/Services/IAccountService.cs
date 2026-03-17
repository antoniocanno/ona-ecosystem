namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task ChangePasswordAsync(string currentPassword, string newPassword);
    }
}

using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.DTOs.Responses;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<AuthResult?> LoginAsync(LoginRequest request);
        Task<AuthResult?> GoogleLoginAsync(GoogleLoginRequest request);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task VerifyEmailAsync(VerifyEmailRequest request);
        Task ResendVerificationEmailAsync(string email);
        Task LogoutAsync(string? refreshToken = null);
        Task LogoutAllAsync(Guid userId);
    }
}

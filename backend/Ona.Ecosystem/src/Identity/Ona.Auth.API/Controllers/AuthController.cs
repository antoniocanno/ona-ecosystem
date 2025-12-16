using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Core.Common.Extensions;
using Ona.ServiceDefaults.ApiExtensions;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowWithoutEmailVerification]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _services;
        private readonly IAccountService _accountService;

        public AuthController(
            IAuthService services,
            IAccountService accountService)
        {
            _services = services;
            _accountService = accountService;
        }

        [HttpPost("register")]
        [RateLimit(10, 10, "register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _services.RegisterAsync(request);
            return Created();
        }

        [HttpPost("login")]
        [RateLimit(10, 10, "login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var authResponse = await _services.LoginAsync(request);
            SetRefreshTokenCookie(authResponse!.RefreshToken, authResponse.RefreshTokenExpires);
            return Ok(authResponse);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var authResponse = await _services.GoogleLoginAsync(request);
            SetRefreshTokenCookie(authResponse!.RefreshToken, authResponse.RefreshTokenExpires);
            return Ok(authResponse);
        }

        [HttpPost("verify-email")]
        [AllowWithoutEmailVerification]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            await _services.VerifyEmailAsync(request);
            return Ok();
        }

        [HttpPost("resend-verification")]
        [RateLimit(3, 10, "email-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
        {
            await _services.ResendVerificationEmailAsync(request.Email);
            return Ok();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = GetRefreshTokenCookie();

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token não encontrado ou inválido no cookie.");

            var authResponse = await _services.RefreshTokenAsync(refreshToken);
            SetRefreshTokenCookie(authResponse.RefreshToken, authResponse.RefreshTokenExpires);
            return Ok(authResponse);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _services.LogoutAsync(request.RefreshToken);
            DeleteRefreshTokenCookie();
            return Ok();
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.GetUserId().ToGuid();
            await _services.LogoutAllAsync(userId);
            DeleteRefreshTokenCookie();
            return Ok();
        }

        [HttpPost("forgot-password")]
        [RateLimit(5, 15, "password-reset")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _accountService.RequestPasswordResetAsync(request.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _accountService.ResetPasswordAsync(request.Token, request.NewPassword);
            return Ok();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.GetUserId();
            await _accountService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return Ok();
        }

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = expires
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string? GetRefreshTokenCookie()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                if (!string.IsNullOrWhiteSpace(refreshToken))
                    return refreshToken;

            return null;
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken");
        }
    }
}

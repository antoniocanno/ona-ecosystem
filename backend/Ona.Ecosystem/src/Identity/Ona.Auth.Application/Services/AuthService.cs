using Google.Apis.Auth;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.DTOs.Responses;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Settings;
using Ona.Auth.Domain.Constants;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Domain.Interfaces.Services;
using Ona.Core.Common.Exceptions;

namespace Ona.Auth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cache;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserDomainService _userDomainService;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly SecuritySettings _securitySettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ICacheService cache,
            IRefreshTokenService refreshTokenService,
            IUserDomainService userDomainService,
            IOptions<GoogleAuthSettings> googleAuthSettings,
            IOptions<JwtSettings> jwtSettings,
            IOptions<SecuritySettings> securitySettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _cache = cache;
            _refreshTokenService = refreshTokenService;
            _userDomainService = userDomainService;
            _googleAuthSettings = googleAuthSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _securitySettings = securitySettings.Value;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var user = request.Adapt<ApplicationUser>();

            // Regra específica de aplicação: usar email como user name se não informado?
            // O DomainService já lida com UserName = Email se vazio, mas aqui o Adapt pode já preencher.

            // O DomainService cuida da criação e erros
            // Mas RegisterAsync original recebia request.Password.
            await _userDomainService.CreateUserAsync(user, request.Password);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailService.SendEmailVerificationAsync(user.Email!, token, user.FullName);
        }

        public async Task<AuthResult?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            await _userDomainService.ValidateUserForLoginAsync(user); // Valida se user é null ou lança exceção

            var result = await _signInManager.CheckPasswordSignInAsync(user!, request.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user!);
                var accessToken = _jwtTokenService.GenerateAccessToken(user!, roles);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user!.Id);

                return new AuthResult
                {
                    AccessToken = accessToken,
                    ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpires = refreshToken.ExpiresAt
                };
            }

            if (result.IsLockedOut)
                throw new ForbiddenException(AuthConstants.Errors.AccountLocked);

            throw new ForbiddenException(AuthConstants.Errors.InvalidCredentials);
        }

        public async Task<AuthResult?> GoogleLoginAsync(GoogleLoginRequest request)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = [_googleAuthSettings.ClientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

            var user = _userManager.Users.FirstOrDefault(u => u.GoogleId == payload.Subject);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);

                if (user != null)
                {
                    user.GoogleId = payload.Subject;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    user = await _userDomainService.CreateGoogleUserAsync(payload.Email, payload.Name, payload.Subject, payload.EmailVerified);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

            return new AuthResult
            {
                AccessToken = accessToken,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpires = refreshToken.ExpiresAt
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenService.GetByTokenAsync(token);

            var user = refreshToken.User;

            await _refreshTokenService.RevokeTokenAsync(refreshToken.Token);

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

            return new AuthResult
            {
                AccessToken = newAccessToken,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpires = newRefreshToken.ExpiresAt
            };
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) throw new ValidationException("Email inválido.");

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
                throw new ValidationException("Token inválido ou expirado.");
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return;

            if (await _userManager.IsEmailConfirmedAsync(user))
                throw new ValidationException("Este email já foi verificado.");

            string counterKey = $"resend_verification_attempts:{email}";
            long currentAttempts = await _cache.IncrementAsync(counterKey, TimeSpan.FromHours(1));

            if (currentAttempts >= _securitySettings.AttemptSettings!.MaxAttemptsPerHour)
                throw new ValidationException("Muitas tentativas. Tente novamente em 1 hora.");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailService.SendEmailVerificationAsync(user.Email!, token, user.FullName);
        }

        public async Task LogoutAsync(string? refreshToken = null)
        {
            if (!string.IsNullOrEmpty(refreshToken))
                await _refreshTokenService.RevokeTokenAsync(refreshToken);

            await _signInManager.SignOutAsync();
        }

        public async Task LogoutAllAsync(Guid userId)
        {
            await _refreshTokenService.RevokeAllUserTokensAsync(userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
                await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}

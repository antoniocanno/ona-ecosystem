using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Services;
using Ona.Auth.Application.Settings;
using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMapster();

            // Configurações
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<SecuritySettings>(configuration.GetSection(nameof(SecuritySettings)));
            services.Configure<PasswordSettings>(configuration.GetSection(nameof(PasswordSettings)));
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.Configure<GoogleAuthSettings>(configuration.GetSection(nameof(GoogleAuthSettings)));

            // Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserAppService, UserService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Tokens
            services.AddTokenService<EmailVerificationToken>();
            services.AddTokenService<PasswordResetToken>();
            services.AddTokenService<RefreshToken>();
            services.AddTokenService<UnlockUserToken>();

            return services;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ona.Auth.Application.Interfaces.Common;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Auth.Infrastructure.Repositories;
using Ona.Auth.Infrastructure.Services;
using Ona.Auth.Infrastructure.Services.Background;

namespace Ona.Auth.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            // Serviços de Infraestrutura
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<IEmailTemplateEngine, RazorEmailTemplateEngine>();
            services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICacheService, RedisCacheService>();

            // Repositórios
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserTenantRoleRepository, UserTenantRoleRepository>();
            services.AddScoped<IApplicationRoleRepository, ApplicationRoleRepository>();
            services.AddScoped<ITenantInviteRepository, TenantInviteRepository>();

            // Tokens
            services.AddTokenRepository<EmailVerificationToken>();
            services.AddTokenRepository<PasswordResetToken>();
            services.AddTokenRepository<RefreshToken>();
            services.AddTokenRepository<UnlockUserToken>();

            // Background Services
            services.AddHostedService<TokenCleanupService>();

            return services;
        }


    }
}

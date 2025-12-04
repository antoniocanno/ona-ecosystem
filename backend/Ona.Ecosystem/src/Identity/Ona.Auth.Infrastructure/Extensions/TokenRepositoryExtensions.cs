using Microsoft.Extensions.DependencyInjection;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Services;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Repositories;

namespace Ona.Auth.Infrastructure.Extensions
{
    public static class TokenRepositoryExtensions
    {
        public static IServiceCollection AddTokenRepository<T>(this IServiceCollection services)
            where T : BaseToken, new()
        {
            services.AddScoped<TokenRepository<T>>();
            services.AddScoped<ICleanupableTokenRepository, TokenRepository<T>>();
            services.AddScoped<ITokenRepository<T>, TokenRepository<T>>();

            services.AddScoped<ITokenService<T>, TokenService<T>>();

            return services;
        }
    }
}

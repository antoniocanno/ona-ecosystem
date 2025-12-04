using Microsoft.Extensions.DependencyInjection;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Services;
using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Extensions
{
    public static class TokenServiceExtensions
    {
        public static IServiceCollection AddTokenService<T>(this IServiceCollection services)
            where T : BaseToken, new()
        {
            services.AddScoped<ITokenService<T>, TokenService<T>>();

            return services;
        }
    }
}

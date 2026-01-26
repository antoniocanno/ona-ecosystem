using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ona.Core.Common.Exceptions;
using System.Text;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:Secret"]
                ?? throw new ConfigurationException("JwtSettings:Secret não configurado no appsettings.json e nem via Environment Variables.");

            var issuer = configuration["JwtSettings:Issuer"]
                ?? throw new ConfigurationException("JwtSettings:Issuer não configurado. Certifique-se que o AppHost está passando este valor.");

            var audience = configuration["JwtSettings:Audience"]
                ?? throw new ConfigurationException("JwtSettings:Audience não configurado. Certifique-se que o AppHost está passando este valor.");

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                };
            });

            return services;
        }
    }
}

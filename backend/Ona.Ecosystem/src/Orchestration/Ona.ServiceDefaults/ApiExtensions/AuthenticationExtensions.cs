using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class AuthenticationExtensions
    {
        // public static IServiceCollection AddJwtAuthentication(
        //     this IServiceCollection services,
        //     IConfiguration configuration)
        // {
        //     var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        //     if (jwtSettings == null)
        //         throw new InvalidOperationException("JwtSettings não configurado no appsettings.json");

        //     services.AddAuthentication(options =>
        //     {
        //         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //     })
        //     .AddJwtBearer(options =>
        //     {
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidateAudience = true,
        //             ValidateLifetime = true,
        //             ValidateIssuerSigningKey = true,

        //             ValidIssuer = jwtSettings.Issuer,
        //             ValidAudience = jwtSettings.Audience,
        //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        //         };
        //     });

        //     services.AddAuthorization();

        //     return services;
        // }
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secretKey = configuration["SSO:Secret"]
                ?? throw new InvalidOperationException("SSO não configurado no appsettings.json");

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

                    ValidIssuer = configuration["SSO:Issuer"],
                    ValidAudience = configuration["SSO:Audience"],
                };
            });

            return services;
        }
    }
}

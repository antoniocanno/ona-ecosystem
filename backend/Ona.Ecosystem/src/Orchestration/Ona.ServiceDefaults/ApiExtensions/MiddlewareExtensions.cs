using Microsoft.AspNetCore.Builder;
using Ona.ServiceDefaults.Middleware;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adiciona middleware de verificação de email para controlar acesso
        /// baseado no status de verificação do email do usuário.
        /// </summary>
        /// <param name="app">Instância do WebApplication</param>
        /// <returns>WebApplication para method chaining</returns>
        public static WebApplication UseEmailVerificationMiddleware(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseMiddleware<EmailVerificationMiddleware>();
            return app;
        }

        /// <summary>
        /// Adiciona middleware de rate limiting para controlar o número de requisições
        /// aos endpoints baseados em atributos aplicados.
        /// </summary>
        /// <param name="app">Instância do WebApplication</param>
        /// <returns>WebApplication para method chaining</returns>
        public static WebApplication UseRateLimitMiddleware(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseMiddleware<RateLimiterMiddleware>();
            return app;
        }
    }
}

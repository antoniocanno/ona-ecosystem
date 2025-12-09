using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Ona.Core.Common.Exceptions;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Configura o middleware de tratamento de exceções e mapeia o endpoint '/error'
        /// para processar a lógica de mapeamento de exceção para Problem Details (RFC 7807).
        /// </summary>
        /// <param name="app">A instância de WebApplication.</param>
        /// <returns>A instância de WebApplication para encadeamento.</returns>
        public static WebApplication UseCustomErrorHandling(this WebApplication app)
        {
            // 1. Configura o middleware para redirecionar exceções para o endpoint /error
            app.UseExceptionHandler("/error");

            // 2. Mapeia o endpoint /error para a lógica de manipulação de exceções
            app.Map("/error", async (context) =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;
                var originalPath = exceptionHandlerPathFeature?.Path;

                var statusCode = exception switch
                {
                    // Exceções de Regra de Negócio / Validação
                    DomainValidationException => StatusCodes.Status400BadRequest,
                    ValidationException => StatusCodes.Status400BadRequest,
                    NotFoundException => StatusCodes.Status404NotFound,
                    ForbiddenException => StatusCodes.Status403Forbidden,

                    // Exceções de Segurança
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

                    // Erro Inesperado
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await Results.Problem(
                    title: exception?.Message ?? "Um erro inesperado ocorreu.",
                    statusCode: statusCode,
                    //type: $"https://ona.com/errors/{exception?.GetType().Name.ToLowerInvariant() ?? "internal-server-error"}",
                    detail: exception?.InnerException?.Message,
                    instance: originalPath
                ).ExecuteAsync(context);
            }).ExcludeFromDescription();

            return app;
        }
    }
}

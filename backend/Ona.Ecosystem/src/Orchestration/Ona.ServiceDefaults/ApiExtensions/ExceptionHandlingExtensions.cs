using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            app.UseExceptionHandler("/error");

            app.Map("/error", async (context) =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;
                var originalPath = exceptionHandlerPathFeature?.Path;

                var loggerFactory = context.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var logger = loggerFactory?.CreateLogger("ExceptionHandler");
                logger?.LogError(exception, "Exceção não tratada no path: {Path}", originalPath);

                var (statusCode, title, detail, shouldExposeDetails) = MapException(exception);

                var env = context.RequestServices.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
                var isDevelopment = env?.IsDevelopment() ?? false;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                if (exception is ServiceUnavailableException serviceException && serviceException.RetryAfterSeconds.HasValue)
                {
                    context.Response.Headers["Retry-After"] = serviceException.RetryAfterSeconds.Value.ToString();
                }

                await Results.Problem(
                    title: title,
                    statusCode: statusCode,
                    type: $"https://ona.com/errors/{GetErrorType(exception)}",
                    detail: shouldExposeDetails ? detail : (isDevelopment ? detail : null),
                    instance: originalPath
                ).ExecuteAsync(context);
            }).ExcludeFromDescription();

            return app;
        }

        /// <summary>
        /// Mapeia a exceção para código HTTP, título e detalhes apropriados.
        /// Retorna também um flag indicando se os detalhes podem ser expostos ao usuário.
        /// </summary>
        private static (int StatusCode, string Title, string? Detail, bool ShouldExposeDetails) MapException(Exception? exception)
        {
            return exception switch
            {
                #region Business Exceptions

                // Validação de domínio e regras de negócio
                DomainValidationException ex => (
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.InnerException?.Message,
                    true
                ),

                ValidationException ex => (
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.InnerException?.Message,
                    true
                ),

                // Recurso não encontrado
                NotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    ex.Message,
                    ex.InnerException?.Message,
                    true
                ),

                // Acesso proibido (autenticado mas sem permissão)
                ForbiddenException ex => (
                    StatusCodes.Status403Forbidden,
                    ex.Message,
                    ex.InnerException?.Message,
                    true
                ),

                // Conflito de recursos (duplicação, estado inválido)
                ConflictException ex => (
                    StatusCodes.Status409Conflict,
                    ex.Message,
                    ex.InnerException?.Message,
                    true
                ),

                #endregion

                #region Security Exceptions

                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Acesso não autorizado. Faça login para continuar.",
                    null,
                    false
                ),

                #endregion

                #region Integration Exceptions

                // Integração com serviços externos
                IntegrationException ex => (
                    ex.IsTransient ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status502BadGateway,
                    $"Erro na integração com {ex.ServiceName}. {(ex.IsTransient ? "Por favor, tente novamente." : "Entre em contato com o suporte.")}",
                    ex.Message,
                    true
                ),

                // Serviço indisponível
                ServiceUnavailableException ex => (
                    StatusCodes.Status503ServiceUnavailable,
                    ex.Message,
                    null,
                    true
                ),

                // Erro de requisição HTTP externa
                HttpRequestException => (
                    StatusCodes.Status502BadGateway,
                    "Erro ao comunicar com serviço externo. Por favor, tente novamente.",
                    null,
                    false
                ),

                #endregion

                #region Infrastructure Exceptions

                // Erro de configuração (não expor detalhes técnicos)
                ConfigurationException => (
                    StatusCodes.Status500InternalServerError,
                    "Erro de configuração do sistema. Entre em contato com o suporte.",
                    null,
                    false
                ),

                // InvalidOperationException - frequentemente contém informações de infraestrutura
                InvalidOperationException => (
                    StatusCodes.Status500InternalServerError,
                    "Operação não permitida no estado atual. Entre em contato com o suporte se o problema persistir.",
                    null,
                    false
                ),

                // ArgumentNullException - erro de programação, não expor
                ArgumentNullException => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor. Entre em contato com o suporte.",
                    null,
                    false
                ),

                // ArgumentException - pode conter informações sensíveis
                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    "Parâmetro inválido fornecido.",
                    null,
                    false
                ),

                // Timeout de operações
                TimeoutException => (
                    StatusCodes.Status504GatewayTimeout,
                    "A operação excedeu o tempo limite. Por favor, tente novamente.",
                    null,
                    false
                ),

                #endregion

                #region Fallback Exceptions

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro inesperado. Entre em contato com o suporte se o problema persistir.",
                    exception?.Message,
                    false
                )

                #endregion
            };
        }

        /// <summary>
        /// Retorna um identificador de tipo de erro para a URL do problem details.
        /// </summary>
        private static string GetErrorType(Exception? exception)
        {
            return exception switch
            {
                DomainValidationException => "domain-validation-error",
                ValidationException => "validation-error",
                NotFoundException => "not-found",
                ForbiddenException => "forbidden",
                ConflictException => "conflict",
                UnauthorizedAccessException => "unauthorized",
                IntegrationException => "integration-error",
                ServiceUnavailableException => "service-unavailable",
                HttpRequestException => "external-service-error",
                ConfigurationException => "configuration-error",
                InvalidOperationException => "invalid-operation",
                TimeoutException => "timeout",
                _ => "internal-server-error"
            };
        }
    }
}

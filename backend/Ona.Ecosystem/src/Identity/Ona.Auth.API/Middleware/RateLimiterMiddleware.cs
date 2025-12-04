using Microsoft.Extensions.Caching.Distributed;
using Ona.Auth.API.Attributes;
using Ona.Auth.Domain.ValueObjects;
using System.Text;
using System.Text.Json;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public RateLimiterMiddleware(
        RequestDelegate next,
        IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Aplicar rate limiting apenas se o endpoint tem o atributo
        var rateLimitAttribute = GetRateLimitAttribute(context);
        if (rateLimitAttribute == null)
        {
            await _next(context);
            return;
        }

        // 2. Identificar o cliente e o endpoint
        var clientIdentifier = GetClientIdentifier(context);
        var endpointIdentifier = GetEndpointIdentifier(context, rateLimitAttribute.Scope);

        // 3. Verificar rate limit (assíncrono)
        if (await IsRateLimitedAsync(clientIdentifier, endpointIdentifier, rateLimitAttribute))
        {
            await RespondWithRateLimitExceeded(context, clientIdentifier, endpointIdentifier, rateLimitAttribute);
            return;
        }

        // 4. Registrar a requisição (assíncrono)
        await RecordRequestAsync(clientIdentifier, endpointIdentifier, rateLimitAttribute);

        await _next(context);
    }

    // --- Métodos de Serialização ---
    private string Serialize(RateLimitInfo info) => JsonSerializer.Serialize(info);
    private RateLimitInfo? Deserialize(string json) => JsonSerializer.Deserialize<RateLimitInfo>(json);
    // --------------------------------

    private static RateLimitAttribute? GetRateLimitAttribute(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();

        // Cria um hash curto do UserAgent para compor a chave
        var userAgentHash = string.IsNullOrEmpty(userAgent)
            ? "no-ua"
            : Convert.ToBase64String(Encoding.UTF8.GetBytes(userAgent))[..16];

        return $"{ip}:{userAgentHash}";
    }

    private static string GetEndpointIdentifier(HttpContext context, string scope)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;
        return $"{scope}:{method}:{path}";
    }

    private async Task<bool> IsRateLimitedAsync(string clientIdentifier, string endpointIdentifier, RateLimitAttribute attribute)
    {
        var cacheKey = $"ratelimit:{clientIdentifier}:{endpointIdentifier}";

        var json = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(json))
        {
            var rateLimitInfo = Deserialize(json);

            if (rateLimitInfo != null)
            {
                // Verifica se o limite foi atingido DENTRO da janela de tempo
                return rateLimitInfo.RequestCount >= attribute.MaxRequests &&
                       DateTime.UtcNow < rateLimitInfo.WindowEnd;
            }
        }

        return false;
    }

    private async Task RecordRequestAsync(string clientIdentifier, string endpointIdentifier, RateLimitAttribute attribute)
    {
        var cacheKey = $"ratelimit:{clientIdentifier}:{endpointIdentifier}";
        var windowSize = TimeSpan.FromMinutes(attribute.WindowMinutes);
        RateLimitInfo rateLimitInfo;

        var json = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(json))
        {
            // Chave expirada ou não existe: Nova janela de tempo
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 1,
                WindowStart = DateTime.UtcNow,
                WindowEnd = DateTime.UtcNow.Add(windowSize)
            };
        }
        else
        {
            // Chave existe
            var existingInfo = Deserialize(json);

            // Verifica a nullabilidade do objeto desserializado
            if (existingInfo == null)
            {
                // Se a desserialização falhar, trata como nova janela para evitar erros
                rateLimitInfo = new RateLimitInfo
                {
                    RequestCount = 1,
                    WindowStart = DateTime.UtcNow,
                    WindowEnd = DateTime.UtcNow.Add(windowSize)
                };
            }
            else if (DateTime.UtcNow >= existingInfo.WindowEnd)
            {
                // Janela de tempo expirada, mas a chave ainda não foi removida pelo TTL: Reinicia.
                rateLimitInfo = new RateLimitInfo
                {
                    RequestCount = 1,
                    WindowStart = DateTime.UtcNow,
                    WindowEnd = DateTime.UtcNow.Add(windowSize)
                };
            }
            else
            {
                // Incrementar contador na janela existente
                rateLimitInfo = existingInfo;
                rateLimitInfo.RequestCount++;
            }
        }

        // Salvar o valor atualizado e definir a expiração no cache distribuído
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = rateLimitInfo.WindowEnd
        };

        var updatedJson = Serialize(rateLimitInfo);

        await _cache.SetStringAsync(cacheKey, updatedJson, options);
    }

    private static async Task RespondWithRateLimitExceeded(
        HttpContext context,
        string clientIdentifier,
        string endpointIdentifier,
        RateLimitAttribute attribute)
    {
        var cacheKey = $"ratelimit:{clientIdentifier}:{endpointIdentifier}";
        var distributedCache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var json = await distributedCache.GetStringAsync(cacheKey);

        RateLimitInfo? Deserialize(string json) => JsonSerializer.Deserialize<RateLimitInfo>(json);

        if (!string.IsNullOrEmpty(json))
        {
            var rateLimitInfo = Deserialize(json);

            if (rateLimitInfo != null)
            {
                var retryAfter = (int)(rateLimitInfo.WindowEnd - DateTime.UtcNow).TotalSeconds;

                // Headers de Resposta Padrão
                context.Response.Headers["Retry-After"] = retryAfter.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = attribute.MaxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = rateLimitInfo.WindowEnd.ToString("R");
                context.Response.Headers["X-RateLimit-Scope"] = attribute.Scope;
            }
        }

        await Results.Problem(
            title: "Muitas requisições",
            statusCode: StatusCodes.Status429TooManyRequests,
            type: "https://ona.com/errors/rate-limit-exceeded",
            detail: $"Muitas tentativas para esta ação. Tente novamente em {attribute.WindowMinutes} minutos.",
            instance: context.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["retry_after_seconds"] = context.Response.Headers["Retry-After"].FirstOrDefault(),
                ["limit"] = attribute.MaxRequests,
                ["window_minutes"] = attribute.WindowMinutes,
                ["scope"] = attribute.Scope
            }
        ).ExecuteAsync(context);
    }
}
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ona.Core.Tenant;
using System.Net.Http.Json;

namespace Ona.Commit.Infrastructure.MultiTenancy
{
    public class HttpTenantProvider : ITenantProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpTenantProvider> _logger;

        public HttpTenantProvider(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<HttpTenantProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TenantContext?> GetAsync(Guid tenantId)
        {
            return await _cache.GetOrCreateAsync($"tenant:{tenantId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var client = _httpClientFactory.CreateClient("Ona.Auth");

                try
                {
                    var response = await client.GetAsync($"api/internal/tenants/{tenantId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            return null;
                        }

                        _logger.LogError("Failed to get tenant context for {TenantId}. Status: {Status}", tenantId, response.StatusCode);
                        throw new InvalidOperationException($"Could not retrieve tenant context for {tenantId}");
                    }

                    var context = await response.Content.ReadFromJsonAsync<TenantContext>();

                    if (context == null)
                        return null;

                    return context;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching tenant context for {TenantId}", tenantId);
                    throw;
                }

            });
        }

        public void Invalidate(Guid tenantId)
        {
            _cache.Remove($"tenant:{tenantId}");
        }
    }
}

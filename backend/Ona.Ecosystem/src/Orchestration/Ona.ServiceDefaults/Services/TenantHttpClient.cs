using Microsoft.Extensions.Caching.Memory;
using Ona.Application.Shared.DTOs.Tenants;
using Ona.Core.Tenant;
using System.Net.Http.Json;

namespace Ona.ServiceDefaults.Services
{
    public class TenantHttpClient : ITenantProvider
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;

        public TenantHttpClient(IMemoryCache cache, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
        }

        public async Task<TenantContext?> GetAsync(Guid tenantId)
        {
            return await _cache.GetOrCreateAsync($"tenant:{tenantId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

                try
                {
                    var dto = await _httpClient.GetFromJsonAsync<TenantDto>($"/api/tenants/{tenantId}");

                    if (dto == null) return null;

                    return new TenantContext
                    {
                        TenantId = dto.Id,
                        Name = dto.Name,
                        TimeZone = dto.TimeZone,
                        WhatsAppApiKey = dto.WhatsAppApiKey
                    };
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            });
        }

        public void Invalidate(Guid tenantId)
        {
            _cache.Remove($"tenant:{tenantId}");
        }
    }
}

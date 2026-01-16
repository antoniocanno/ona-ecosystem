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

        public async Task<TenantContext> GetAsync(Guid tenantId)
        {
            return await _cache.GetOrCreateAsync($"tenant:{tenantId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

                var dto = await _httpClient.GetFromJsonAsync<TenantDto>($"/api/tenants/{tenantId}");

                return new TenantContext
                {
                    TenantId = dto!.Id,
                    Name = dto.Name,
                    TimeZone = dto.TimeZone,
                    WhatsAppApiKey = dto.WhatsAppApiKey
                };
            }) ?? throw new InvalidOperationException("Não foi possível recuperar o contexto do cliente");
        }

        public void Invalidate(Guid tenantId)
        {
            _cache.Remove($"tenant:{tenantId}");
        }
    }
}

using System.Net.Http.Json;
using Ona.Application.Shared.DTOs.Tenants;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Core.Entities;

namespace Ona.Commit.Infrastructure.Services
{
    public class TenantHttpClient : ITenantService
    {
        private readonly HttpClient _httpClient;

        public TenantHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TenantDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<TenantDto>($"/api/Tenants/{id}");
        }

        public async Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Tenants/{id}", request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TenantDto>())!;
        }

        public async Task<IEnumerable<TenantDto>> ListAsync()
        {
            return (await _httpClient.GetFromJsonAsync<IEnumerable<TenantDto>>("/api/Tenants"))!;
        }

        public async Task SuspendAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"/api/Tenants/{id}/suspend", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"/api/Tenants/{id}/activate", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"/api/Tenants/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Tenants", request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<Tenant>())!;
        }
    }
}

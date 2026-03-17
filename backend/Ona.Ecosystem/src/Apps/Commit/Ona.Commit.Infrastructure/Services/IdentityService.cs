using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ona.Commit.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(IHttpClientFactory httpClientFactory, ILogger<IdentityService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Ona.Auth");
            _logger = logger;
        }

        public async Task<Guid> CreateUserAsync(string email, string fullName, string password, string role)
        {
            var request = new
            {
                Email = email,
                FullName = fullName,
                Password = password,
                Role = role
            };

            var response = await _httpClient.PostAsJsonAsync("api/users", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating user in Identity: {Error}", error);
                throw new IntegrationException("Identity", $"Erro ao criar usuário: {response.StatusCode}", isTransient: false);
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetGuid();
        }
    }
}

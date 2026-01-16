using Microsoft.Extensions.Configuration;

namespace Ona.Infrastructure.Shared.Integrations
{
    public sealed class InternalApiKeyHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;

        public InternalApiKeyHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("X-Internal-Api-Key", _configuration["Auth:InternalApiKey"]);

            return base.SendAsync(request, cancellationToken);
        }
    }
}

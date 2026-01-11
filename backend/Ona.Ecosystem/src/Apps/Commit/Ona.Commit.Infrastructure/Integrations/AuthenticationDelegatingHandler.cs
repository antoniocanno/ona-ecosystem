using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && AuthenticationHeaderValue.TryParse(authHeader, out var headerValue))
            {
                request.Headers.Authorization = headerValue;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

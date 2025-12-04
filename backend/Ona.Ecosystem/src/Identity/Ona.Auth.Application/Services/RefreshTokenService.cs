using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Common;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Settings;
using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Services
{
    public class RefreshTokenService : TokenService<RefreshToken>, IRefreshTokenService
    {
        private readonly SecuritySettings _securitySettings;
        private readonly ITokenGenerator _tokenGenerator;

        public RefreshTokenService(
            ITokenRepository<RefreshToken> tokenRepository,
            ITokenGenerator tokenGenerator,
            IOptions<SecuritySettings> securitySettings)
            : base(tokenRepository, tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
            _securitySettings = securitySettings.Value;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
        {
            var activeTokens = await GetUserTokenCountAsync(userId);
            if (activeTokens >= _securitySettings.MaximumRefreshTokensPerUser)
            {
                await RevokeAllUserTokensAsync(userId);
            }

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = _tokenGenerator.GenerateSecureToken(64),
                ExpiresAt = DateTime.UtcNow.AddDays(_securitySettings.RefreshTokenExpiryDays)
            };

            refreshToken = await CreateAsync(refreshToken);

            return refreshToken;
        }
    }
}

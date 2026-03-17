using Ona.Auth.Application.Interfaces.Common;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Domain.Entities;
using Ona.Core.Common.Exceptions;

namespace Ona.Auth.Application.Services
{
    public class TokenService<T> : ITokenService<T> where T : BaseToken, new()
    {
        protected readonly ITokenRepository<T> _tokenRepository;
        private readonly ITokenGenerator _tokenGenerator;

        public TokenService(
            ITokenRepository<T> tokenRepository,
            ITokenGenerator tokenGenerator)
        {
            _tokenRepository = tokenRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<T> GetByTokenAsync(string token)
        {
            var storedToken = await _tokenRepository.GetByTokenAsync(token);

            if (storedToken == null || !storedToken.IsActive)
                throw new ForbiddenException("Token inválido ou expirado");

            return storedToken;
        }

        public async Task<T> CreateAsync(Guid userId, TimeSpan validityDuration, int tokenLength = 32)
        {
            var token = new T
            {
                UserId = userId,
                Token = _tokenGenerator.GenerateSecureToken(tokenLength),
                ExpiresAt = DateTime.UtcNow.Add(validityDuration)
            };

            token = await CreateAsync(token);
            return token;
        }

        public async Task<T> CreateAsync(T token)
        {
            token = await _tokenRepository.CreateAsync(token);
            await _tokenRepository.SaveChangesAsync();
            return token;
        }

        public async Task RevokeTokenAsync(string token)
        {
            var storedToken = await GetByTokenAsync(token);
            if (storedToken != null)
            {
                storedToken.Revoke();
                _tokenRepository.Update(storedToken);
                await _tokenRepository.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            await _tokenRepository.RevokeAllUserTokensAsync(userId);
        }

        public async Task<int> GetUserTokenCountAsync(Guid userId)
        {
            return await _tokenRepository.GetUserTokenCountAsync(userId);
        }
    }
}
using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Common;
using Ona.Auth.Application.Settings;
using System.Globalization;
using System.Security.Cryptography;

namespace Ona.Auth.Infrastructure.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly SecuritySettings _securitySettings;

        public TokenGenerator(IOptions<SecuritySettings> securitySettings)
        {
            _securitySettings = securitySettings.Value;
        }

        public string GenerateVerificationToken()
        {
            return GenerateTimeBasedToken();
        }

        public string GenerateTimeBasedToken()
        {
            var expiresAt = DateTime.UtcNow.AddHours(_securitySettings.EmailVerificationTokenExpiryHours).Ticks;
            var randomPart = GenerateSecureToken(_securitySettings.SecureRandomTokenLength);

            return $"{expiresAt:x}.{randomPart}";
        }

        public string GenerateSecureToken(int length = 32)
        {
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public bool TryExtractTicksFromToken(string token, out long ticks)
        {
            ticks = 0;

            if (string.IsNullOrEmpty(token))
                return false;

            var parts = token.Split('.', 2);
            if (parts.Length != 2)
                return false;

            return long.TryParse(
                parts[0],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out ticks);
        }
    }
}

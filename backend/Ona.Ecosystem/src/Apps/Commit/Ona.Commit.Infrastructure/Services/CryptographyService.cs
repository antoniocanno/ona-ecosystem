using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Ona.Commit.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Ona.Commit.Infrastructure.Services
{
    public class CryptographyService : ICryptographyService
    {
        private readonly string _key;

        public CryptographyService(IConfiguration configuration)
        {
            _key = configuration["Cryptography:Key"] ?? throw new ArgumentNullException("Cryptography:Key configuration is missing.");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            byte[] key = Convert.FromHexString(_key);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            Span<byte> nonce = stackalloc byte[12];
            Span<byte> tag = stackalloc byte[16];
            RandomNumberGenerator.Fill(nonce);

            byte[] cipherBytes = new byte[plainBytes.Length];

            using var aesGcm = new AesGcm(key, tagSizeInBytes: 16);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            byte[] result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
            nonce.CopyTo(result.AsSpan(0, 12));
            tag.CopyTo(result.AsSpan(12, 16));
            cipherBytes.CopyTo(result.AsSpan(28));

            return WebEncoders.Base64UrlEncode(result);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] fullCipher = WebEncoders.Base64UrlDecode(cipherText);
            byte[] key = Convert.FromHexString(_key);

            ReadOnlySpan<byte> cipherSpan = fullCipher.AsSpan();
            ReadOnlySpan<byte> nonce = cipherSpan.Slice(0, 12);
            ReadOnlySpan<byte> tag = cipherSpan.Slice(12, 16);
            ReadOnlySpan<byte> actualCiphertext = cipherSpan.Slice(28);

            byte[] plainBytes = new byte[actualCiphertext.Length];

            using var aesGcm = new AesGcm(key, tagSizeInBytes: 16);
            aesGcm.Decrypt(nonce, actualCiphertext, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}

using System.Text.Json.Serialization;

namespace Ona.Auth.Application.DTOs.Responses
{
    public record AuthResult
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
        [JsonIgnore]
        public string RefreshToken { get; init; } = string.Empty;
        [JsonIgnore]
        public DateTime RefreshTokenExpires { get; init; }
    }
}

using System.Text.Json.Serialization;

namespace Ona.Commit.Infrastructure.Gateways.Evolution.Events
{
    public class EvolutionMessageEvent
    {
        [JsonPropertyName("instance")]
        public string Instance { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object Data { get; set; } = new();

        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("sender")]
        public string? Sender { get; set; }
    }
}

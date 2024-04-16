using System.Text.Json.Serialization;

namespace LAPS_WebUI.Models
{
    public class MsLapsPayload
    {
        [JsonPropertyName("n")]
        public string? ManagedAccountName { get; set; }

        [JsonPropertyName("t")]
        public string? PasswordUpdateTime { get; set; }

        [JsonPropertyName("p")]
        public string? Password { get; set; }
    }
}

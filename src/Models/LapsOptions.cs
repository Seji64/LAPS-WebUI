using LAPS_WebUI.Enums;
using System.Text.Json.Serialization;

namespace LAPS_WebUI.Models
{
    public class LapsOptions
    {
        [JsonConverter(typeof(JsonStringEnumMemberConverter))]
        public LAPSVersion ForceVersion { get; set; } = LAPSVersion.All;
        public bool EncryptionDisabled { get; set; }
        
        public string DateDisplayFormat { get; set; } = "dd.MM.yyyy HH:mm:ss";
    }
}

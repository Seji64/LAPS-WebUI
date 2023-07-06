using LAPS_WebUI.Enums;

namespace LAPS_WebUI.Models
{
    public class LapsOptions
    {
        public LAPSVersion? ForceVersion { get; set; }
        public bool EncryptionDisabled { get; set; }
    }
}

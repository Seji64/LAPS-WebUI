using LAPS_WebUI.Enums;

namespace LAPS_WebUI.Models
{
    public class LapsInformation
    {
        public required string ComputerName { get; set; }
        public string? Password { get; set; }
        public string? Account { get; set; }
        public DateTime? PasswordExpireDate { get; set; }
        public DateTime? PasswordSetDate { get; set; }
        public LAPSVersion? Version { get; set; }
        public bool IsCurrent { get; set; }
        public bool WasEncrypted { get; set; }
    }
}

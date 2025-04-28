using LdapForNet.Native;

namespace LAPS_WebUI.Models
{
    public class LdapOptions
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public bool TrustAllCertificates { get; set; }
        public string? SearchBase { get; set; }
        public string AuthMechanism { get; set; } = "SIMPLE";
    }
}

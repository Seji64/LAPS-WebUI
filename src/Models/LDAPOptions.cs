namespace LAPS_WebUI.Models
{
    public class LdapOptions
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public bool TrustAllCertificates { get; set; }
        public string? SearchBase { get; set; }
    }
}

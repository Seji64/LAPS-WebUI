using LdapForNet;

namespace LAPS_WebUI.Models
{
    public record UserSession(string DomainName, LdapCredential Credential);
}

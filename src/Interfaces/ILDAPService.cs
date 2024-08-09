using LAPS_WebUI.Enums;
using LAPS_WebUI.Models;
using LdapForNet;

namespace LAPS_WebUI.Interfaces
{
    public interface ILdapService
    {
        Task<LdapConnection?> CreateBindAsync(string domainName, string username, string password);
        Task<bool> TestCredentialsAsync(string domainName, string username, string password);
        Task<bool> TestCredentialsAsync(string domainName, LdapCredential ldapCredential);
        Task<List<Domain>> GetDomainsAsync();
        public Task<AdComputer?> GetAdComputerAsync(string domainName, LdapCredential ldapCredential, string distinguishedName);
        public Task<List<AdComputer>> SearchAdComputersAsync(string domainName, LdapCredential ldapCredential, string query);
        public Task<bool> ClearLapsPassword(string domainName, LdapCredential ldapCredential, string distinguishedName, LAPSVersion version);
    }
}

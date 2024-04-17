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
        public Task<ADComputer?> GetADComputerAsync(string domainName, LdapCredential ldapCredential, string distinguishedName);
        public Task<List<ADComputer>> SearchADComputersAsync(string domainName, LdapCredential ldapCredential, string query);
        public Task<bool> ClearLapsPassword(string domainName, LdapCredential ldapCredential, string distinguishedName, LAPSVersion version, bool encrypted);
    }
}

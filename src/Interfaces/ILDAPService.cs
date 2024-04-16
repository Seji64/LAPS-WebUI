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
        public Task<ADComputer?> GetADComputerAsync(string domainName, LdapCredential ldapCredential, string name);
        public Task<List<ADComputer>> SearchADComputersAsync(string domainName, LdapCredential ldapCredential, string query);
    }
}

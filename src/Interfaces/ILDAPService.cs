using LAPS_WebUI.Models;
using LdapForNet;

namespace LAPS_WebUI.Interfaces
{
    public interface ILdapService
    {
        Task<LdapConnection?> CreateBindAsync(string username, string password);
        Task<bool> TestCredentialsAsync(string username, string password);
        Task<bool> TestCredentialsAsync(LdapCredential ldapCredential);
        public Task<ADComputer?> GetADComputerAsync(LdapCredential ldapCredential, string name);
        public Task<List<ADComputer>> SearchADComputersAsync(LdapCredential ldapCredential, string query);
    }
}

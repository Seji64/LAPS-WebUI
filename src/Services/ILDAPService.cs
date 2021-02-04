using LAPS_WebUI.Models;
using LdapForNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Services
{
    public interface ILDAPService
    {
        Task<LdapConnection> CreateBindAsync(string username, string password);
        Task<bool> TestCredentialsAsync(string username, string password);
        Task<bool> TestCredentialsAsync(LdapCredential ldapCredential);
        public Task<ADComputer> GetADComputerAsync(LdapCredential ldapCredential, string name);
        public Task<List<ADComputer>> SearchADComputersAsync(LdapCredential ldapCredential, string query);
    }
}

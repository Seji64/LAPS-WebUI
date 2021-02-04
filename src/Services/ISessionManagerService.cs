using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Services
{
    public interface ISessionManagerService
    {
        public Task<bool> IsUserLoggedInAsync();
        public Task<bool> LoginAsync(string username, string password);
        public Task<bool> LogoutAsync();
        public Task<LdapForNet.LdapCredential> GetLdapCredentialsAsync();
    }
}

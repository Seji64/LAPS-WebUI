using Blazored.SessionStorage;
using LdapForNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Services
{
    public class SessionManagerService : ISessionManagerService
    {

        private ISessionStorageService _sessionStorageService;
        private ILDAPService _ldapService;
        private ICryptService _cryptService;

        public SessionManagerService(ISessionStorageService sessionStorageService, ILDAPService ldapService, ICryptService cryptService)
        {
            _sessionStorageService = sessionStorageService;
            _ldapService = ldapService;
            _cryptService = cryptService;
        }

        public async Task<LdapCredential> GetLdapCredentialsAsync()
        {
            var encryptedCreds = await _sessionStorageService.GetItemAsync<LdapCredential>("ldapCredentials");

            encryptedCreds.UserName = _cryptService.DecryptString(encryptedCreds.UserName);
            encryptedCreds.Password = _cryptService.DecryptString(encryptedCreds.Password);

            return encryptedCreds;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            return await _sessionStorageService.GetItemAsync<bool>("loggedIn");
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var bindResult = await _ldapService.TestCredentialsAsync(username, password);

            if (!bindResult)
            {
                return false;
            }
            else
            {
                await _sessionStorageService.SetItemAsync("loggedIn", bindResult);
                await _sessionStorageService.SetItemAsync("ldapCredentials", new LdapCredential() { UserName = _cryptService.EncryptString(username), Password = _cryptService.EncryptString(password) });

                return true;
            }
        }
        public async Task<bool> LogoutAsync()
        {
            await _sessionStorageService.ClearAsync();
            return true;
        }
    }
}

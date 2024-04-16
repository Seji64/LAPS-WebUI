using Blazored.SessionStorage;
using LAPS_WebUI.Interfaces;
using LdapForNet;

namespace LAPS_WebUI.Services
{
    public class SessionManagerService : ISessionManagerService
    {

        private readonly ISessionStorageService _sessionStorageService;
        private readonly ILdapService _ldapService;
        private readonly ICryptService _cryptService;

        public SessionManagerService(ISessionStorageService sessionStorageService, ILdapService ldapService, ICryptService cryptService)
        {
            _sessionStorageService = sessionStorageService;
            _ldapService = ldapService;
            _cryptService = cryptService;
        }

        public async Task<List<string>> GetDomainsAsync()
        {
            return (await _ldapService.GetDomainsAsync()).Select(x => x.Name).ToList();
        }

        public async Task<string> GetDomainAsync()
        {
            return await _sessionStorageService.GetItemAsync<string>("domainName");
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

        public async Task<bool> LoginAsync(string domainName, string username, string password)
        {
            var bindResult = await _ldapService.TestCredentialsAsync(domainName,username, password);

            if (!bindResult)
            {
                return false;
            }
            else
            {
                await _sessionStorageService.SetItemAsync("loggedIn", bindResult);
                await _sessionStorageService.SetItemAsync("domainName", domainName);
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

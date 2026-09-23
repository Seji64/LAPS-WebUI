using Blazored.SessionStorage;
using LdapForNet;

namespace LAPS_WebUI.Services
{
    public class SessionManagerService(
        ISessionStorageService sessionStorageService,
        LdapService ldapService,
        CryptService cryptService)
    {

        public async Task<string> GetUsernameAsync()
        {
            return await sessionStorageService.GetItemAsync<string>("username");
        }

        public List<string> GetDomains()
        {
            return ldapService.GetDomains().Select(x => x.Name).ToList();
        }

        public async Task<string> GetDomainAsync()
        {
            return await sessionStorageService.GetItemAsync<string>("domainName");
        }

        public async Task<LdapCredential> GetLdapCredentialsAsync()
        {
            LdapCredential? encryptedCreds = await sessionStorageService.GetItemAsync<LdapCredential>("ldapCredentials");

            encryptedCreds.UserName = cryptService.DecryptString(encryptedCreds.UserName);
            encryptedCreds.Password = cryptService.DecryptString(encryptedCreds.Password);

            return encryptedCreds;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            return await sessionStorageService.GetItemAsync<bool>("loggedIn");
        }

        public async Task<bool> LoginAsync(string domainName, string username, string password)
        {
            if (!await ldapService.TestCredentialsAsync(domainName, username, password))
            {
                return false;
            }

            await sessionStorageService.SetItemAsync("loggedIn", true);
            await sessionStorageService.SetItemAsync("username", username);
            await sessionStorageService.SetItemAsync("domainName", domainName);
            await sessionStorageService.SetItemAsync("ldapCredentials", new LdapCredential() { UserName = cryptService.EncryptString(username), Password = cryptService.EncryptString(password) });

            return true;
        }
        public async Task<bool> LogoutAsync()
        {
            await sessionStorageService.ClearAsync();
            return true;
        }
    }
}

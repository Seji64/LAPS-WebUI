using Blazored.SessionStorage;
using LAPS_WebUI.Interfaces;
using LdapForNet;

namespace LAPS_WebUI.Services
{
    public class SessionManagerService(
        ISessionStorageService sessionStorageService,
        ILdapService ldapService,
        ICryptService cryptService)
        : ISessionManagerService
    {
        public async Task<List<string>> GetDomainsAsync()
        {
            return (await ldapService.GetDomainsAsync()).Select(x => x.Name).ToList();
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
            bool bindResult = await ldapService.TestCredentialsAsync(domainName,username, password);

            if (!bindResult)
            {
                return false;
            }
            else
            {
                await sessionStorageService.SetItemAsync("loggedIn", bindResult);
                await sessionStorageService.SetItemAsync("domainName", domainName);
                await sessionStorageService.SetItemAsync("ldapCredentials", new LdapCredential() { UserName = cryptService.EncryptString(username), Password = cryptService.EncryptString(password) });

                return true;
            }
        }
        public async Task<bool> LogoutAsync()
        {
            await sessionStorageService.ClearAsync();
            return true;
        }
    }
}

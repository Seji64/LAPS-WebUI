using Blazored.SessionStorage;
using LAPS_WebUI.Models;
using LdapForNet;

namespace LAPS_WebUI.Services
{
    public class SessionManagerService(
        ISessionStorageService sessionStorageService,
        LdapService ldapService,
        SessionStore sessionStore)
    {
        private const string SessionIdKey = "sessionId";
        private string? _sessionId;

        public List<string> GetDomains()
        {
            return ldapService.GetDomains().Select(x => x.Name).ToList();
        }

        // Null once the session expired or was logged out, also from another tab sharing the ID
        public async Task<UserSession?> GetSessionAsync()
        {
            _sessionId ??= await sessionStorageService.GetItemAsync<string>(SessionIdKey);
            return _sessionId is null ? null : sessionStore.Get(_sessionId);
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            return await GetSessionAsync() is not null;
        }

        public async Task<bool> LoginAsync(string domainName, string username, string password)
        {
            if (!await ldapService.TestCredentialsAsync(domainName, username, password))
            {
                return false;
            }

            await LogoutAsync();
            _sessionId = sessionStore.Create(new UserSession(domainName, new LdapCredential { UserName = username, Password = password }));
            await sessionStorageService.SetItemAsync(SessionIdKey, _sessionId);

            return true;
        }

        public async Task LogoutAsync()
        {
            _sessionId ??= await sessionStorageService.GetItemAsync<string>(SessionIdKey);
            if (_sessionId is not null)
            {
                sessionStore.Remove(_sessionId);
                _sessionId = null;
            }

            await sessionStorageService.ClearAsync();
        }
    }
}

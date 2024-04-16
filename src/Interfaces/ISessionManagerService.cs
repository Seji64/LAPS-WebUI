namespace LAPS_WebUI.Interfaces
{
    public interface ISessionManagerService
    {
        public Task<bool> IsUserLoggedInAsync();
        public Task<bool> LoginAsync(string domainName, string username, string password);
        public Task<bool> LogoutAsync();
        public Task<LdapForNet.LdapCredential> GetLdapCredentialsAsync();
        public Task<List<string>> GetDomainsAsync();
        public Task<string> GetDomainAsync();
    }
}

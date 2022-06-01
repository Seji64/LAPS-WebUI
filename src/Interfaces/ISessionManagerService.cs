namespace LAPS_WebUI.Interfaces
{
    public interface ISessionManagerService
    {
        public Task<bool> IsUserLoggedInAsync();
        public Task<bool> LoginAsync(string username, string password);
        public Task<bool> LogoutAsync();
        public Task<LdapForNet.LdapCredential> GetLdapCredentialsAsync();
    }
}

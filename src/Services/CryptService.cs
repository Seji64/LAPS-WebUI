using LAPS_WebUI.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace LAPS_WebUI.Services
{
    public class CryptService : ICryptService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly string keyString;
        public CryptService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
            keyString = Guid.NewGuid().ToString().Replace("-", "");
        }
        public string DecryptString(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(keyString);
            return protector.Unprotect(cipherText);
        }

        public string EncryptString(string text)
        {
            var protector = _dataProtectionProvider.CreateProtector(keyString);
            return protector.Protect(text);
        }
    }
}

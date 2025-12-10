using LAPS_WebUI.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace LAPS_WebUI.Services
{
    public class CryptService(IDataProtectionProvider dataProtectionProvider) : ICryptService
    {
        private readonly string _keyString = Guid.NewGuid().ToString().Replace("-", "");

        public string DecryptString(string cipherText)
        {
            IDataProtector protector = dataProtectionProvider.CreateProtector(_keyString);
            return protector.Unprotect(cipherText);
        }

        public string EncryptString(string text)
        {
            IDataProtector protector = dataProtectionProvider.CreateProtector(_keyString);
            return protector.Protect(text);
        }
    }
}

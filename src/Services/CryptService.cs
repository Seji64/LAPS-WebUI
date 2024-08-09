using LAPS_WebUI.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace LAPS_WebUI.Services
{
    public class CryptService : ICryptService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly string _keyString;
        public CryptService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _keyString = Guid.NewGuid().ToString().Replace("-", "");
        }
        public string DecryptString(string cipherText)
        {
            IDataProtector protector = _dataProtectionProvider.CreateProtector(_keyString);
            return protector.Unprotect(cipherText);
        }

        public string EncryptString(string text)
        {
            IDataProtector protector = _dataProtectionProvider.CreateProtector(_keyString);
            return protector.Protect(text);
        }
    }
}

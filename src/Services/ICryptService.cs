namespace LAPS_WebUI.Services
{
    public interface ICryptService
    {
        public string EncryptString(string text);

        public string DecryptString(string cipherText);
    }
}

namespace LAPS_WebUI.Interfaces
{
    public interface ICryptService
    {
        public string EncryptString(string text);
        public string DecryptString(string cipherText);
    }
}

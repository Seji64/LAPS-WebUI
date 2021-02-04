using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LAPS_WebUI.Services
{
    public class CryptService : ICryptService
    {
        private readonly string key;
        private readonly string salt;
        public CryptService()
        {
            key = Guid.NewGuid().ToString();
            salt = Guid.NewGuid().ToString();
        }

        private static byte[] AESEncryptBytes(byte[] clearBytes, byte[] passBytes, byte[] saltBytes)
        {
            byte[] encryptedBytes = null;

            // create a key from the password and salt, use 32K iterations – see note
            var key = new Rfc2898DeriveBytes(passBytes, saltBytes, 32768);

            // create an AES object
            using (Aes aes = new AesManaged())
            {
                // set the key size to 256
                aes.KeySize = 256;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        private static byte[] AESDecryptBytes(byte[] cryptBytes, byte[] passBytes, byte[] saltBytes)
        {
            byte[] clearBytes = null;

            // create a key from the password and salt, use 32K iterations
            var key = new Rfc2898DeriveBytes(passBytes, saltBytes, 32768);

            using (Aes aes = new AesManaged())
            {
                // set the key size to 256
                aes.KeySize = 256;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cryptBytes, 0, cryptBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    clearBytes = ms.ToArray();
                }
            }
            return clearBytes;
        }

        public string DecryptString(string cipherText)
        {
            byte[] cryptBytes = Convert.FromBase64String(cipherText);
            byte[] passBytes = Encoding.UTF8.GetBytes(key);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            return Encoding.UTF8.GetString(AESDecryptBytes(cryptBytes, passBytes, saltBytes));
        }

        public string EncryptString(string text)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(text);
            byte[] passBytes = Encoding.UTF8.GetBytes(key);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            return Convert.ToBase64String(AESEncryptBytes(clearBytes, passBytes, saltBytes));
        }
    }
}

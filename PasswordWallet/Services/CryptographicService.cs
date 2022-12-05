using System.Security.Cryptography;
using System.Text;

namespace PasswordWallet.Services;

public class CryptographicService : ICryptographicService
{
    public string Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aesAlgorithm = Aes.Create())
        {
            aesAlgorithm.Key = key;
            aesAlgorithm.IV = iv;

            // Create encryptor object
            ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();

            byte[] encryptedData;

            //Encryption will be done in a memory stream through a CryptoStream object
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    encryptedData = ms.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedData);
        }
    }

    public string Decrypt(string cipherText, byte[] key, byte[] iv)
    {
        using (Aes aesAlgorithm = Aes.Create())
        {
            aesAlgorithm.Key = key;
            aesAlgorithm.IV = iv;
            
            // Create decryptor object
            ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor();

            byte[] cipher = Convert.FromBase64String(cipherText);

            //Decryption will be done in a memory stream through a CryptoStream object
            using (MemoryStream ms = new MemoryStream(cipher))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
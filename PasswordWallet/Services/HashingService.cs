using System.Security.Cryptography;

namespace PasswordWallet.Services;

public class HashingService : IHashingService
{
    public string HashWithSHA512(string text)
    {
        text += "16characterslong";
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);

        byte[] hashValue;

        using (var sha512 = SHA512.Create())
        {
            hashValue = sha512.ComputeHash(bytes);
        }

        return Convert.ToBase64String(hashValue);
    }

    public string HashWithHMAC(string text, string key)
    {
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

        byte[] hashValue;

        using (var hmacSha512 = new HMACSHA512(keyBytes))
        {
            hashValue = hmacSha512.ComputeHash(textBytes);
        }

        return Convert.ToBase64String(hashValue);
    }

    public string HashWithMD5(string text)
    {
        using var md5 = MD5.Create();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
        byte[] hashValue = md5.ComputeHash(bytes);

        return Convert.ToBase64String(hashValue);
    }
}
namespace PasswordWallet.Services;

public interface IHashingService
{
    public string HashWithSHA512(string text);
    public string HashWithHMAC(string text, string key);
    public string HashWithMD5(string text);
}
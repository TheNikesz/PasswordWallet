namespace PasswordWallet.Services;

public interface ICryptographicService
{
    public string Encrypt(string text, byte[] key, byte[] iv);
    public string Decrypt(string cipher, byte[] key, byte[] iv);
}
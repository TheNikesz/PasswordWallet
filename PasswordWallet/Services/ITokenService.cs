using PasswordWallet.Entities;

namespace PasswordWallet.Services
{
    public interface ITokenService
    {
        string CreateToken(Account account);
    }
}
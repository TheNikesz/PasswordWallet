using PasswordWallet.DTOs.Account;
using PasswordWallet.Entities;

namespace PasswordWallet.Services;

public interface IAccountService
{
    public Task<bool> IfAccountExists(string login);
    public Task<Account?> GetAccount(string login);

    public Task<AccountDto> Login(LoginDto loginDto);
    public Task<AccountDto> AddAccount(RegisterDto registerDto);
    public Task<AccountDto> ChangePassword(Guid id, string newPassword, bool isPasswordKeptAsHash);
    public Task<bool> CheckPasswordWithId(Guid id, string password);
    public Task<bool> CheckPasswordWithLogin(string login, string password);
}
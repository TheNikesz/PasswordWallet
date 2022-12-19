using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PasswordWallet.DTOs.Account;
using PasswordWallet.Entities;

namespace PasswordWallet.Services;

public class AccountService : IAccountService
{
    private readonly DataBaseContext _dataContext;
    private readonly ITokenService _tokenService;
    private readonly IHashingService _hashService;
    private readonly ICryptographicService _cryptoService;
    private readonly ISavedPasswordService _savedPasswordService;

    public AccountService(DataBaseContext dataContext, ITokenService tokenService, IHashingService hashService, ICryptographicService cryptoService, ISavedPasswordService savedPasswordService)
    {
        _dataContext = dataContext;
        _tokenService = tokenService;
        _hashService = hashService;
        _cryptoService = cryptoService;
        _savedPasswordService = savedPasswordService;
    }

    public async Task<bool> IfAccountExists(string login)
    {
        return await _dataContext.Accounts.AnyAsync(x => x.Login == login);
    }

    public async Task<Account?> GetAccount(string login)
    {
        return await _dataContext.Accounts.FirstOrDefaultAsync(x => x.Login == login);
    }

    public async Task<AccountDto> Login(LoginDto loginDto)
    {
        var account = await _dataContext.Accounts.FirstAsync(x => x.Login == loginDto.Login);
        return new AccountDto
        {
            Login = loginDto.Login,
            Token = _tokenService.CreateToken(account),
        };
    }

    public async Task<AccountDto> AddAccount(RegisterDto registerDto)
    {
        var salt = GenerateSalt();
        var passwordHash = GetPasswordHash(registerDto.Password, salt, registerDto.IsPasswordKeptAsHash);
        var account = new Account
        {
            Login = registerDto.Login,
            PasswordHash = passwordHash,
            IsPasswordKeptAsHash = registerDto.IsPasswordKeptAsHash,
            Salt = salt,
        };

        await _dataContext.Accounts.AddAsync(account);
        var result = await _dataContext.SaveChangesAsync();
        if (result <= 0) throw new Exception();
        return new AccountDto
        {
            Login = account.Login,
            Token = _tokenService.CreateToken(account),
        };
    }

    public async Task<AccountDto> ChangePassword(Guid id, string newPassword, bool isPasswordKeptAsHash)
    {
        var account = await _dataContext.Accounts.FirstAsync(x => x.Id == id);
        var salt = GenerateSalt();
        var passwordHash = GetPasswordHash(newPassword, salt, isPasswordKeptAsHash);

        var savedPasswords = await _dataContext.SavedPasswords.Where(x => x.AccountId == account.Id).ToListAsync();
        foreach (var savedPassword in savedPasswords)
        {
            savedPassword.Password = await _savedPasswordService.DecryptPassword(savedPassword.Id);
        }

        account.PasswordHash = passwordHash;
        account.Salt = salt;
        account.IsPasswordKeptAsHash = isPasswordKeptAsHash;

        foreach (var savedPassword in savedPasswords)
        {
            var masterPasswordBytes = GetMasterPasswordBytes(passwordHash);
            using var aes = Aes.Create();
            var ivBytes = aes.IV;
            savedPassword.Password = _cryptoService.Encrypt(savedPassword.Password, masterPasswordBytes, ivBytes);
            savedPassword.Iv = Convert.ToBase64String(ivBytes);
        }

        var result = await _dataContext.SaveChangesAsync();
        if (result <= 0) throw new Exception();
        return new AccountDto
        {
            Login = account.Login,
            Token = _tokenService.CreateToken(account),
        };
    }

    public async Task<bool> CheckPasswordWithId(Guid id, string password)
    {
        var account = await _dataContext.Accounts.FirstAsync(x => x.Id == id);
        var hash = GetPasswordHash(password, account.Salt, account.IsPasswordKeptAsHash);
        return hash == account.PasswordHash;
    }

    public async Task<bool> CheckPasswordWithLogin(string login, string password)
    {
        var account = await _dataContext.Accounts.FirstAsync(x => x.Login == login);
        var hash = GetPasswordHash(password, account.Salt, account.IsPasswordKeptAsHash);
        return hash == account.PasswordHash;
    }

    private string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(128 / 8);
        return Convert.ToBase64String(salt);
    }

    private string GetPasswordHash(string password, string salt, bool isPasswordKeptAsHash)
    {
        return isPasswordKeptAsHash
            ? _hashService.HashWithSHA512(password + salt)
            : _hashService.HashWithHMAC(password, salt);
    }

    private byte[] GetMasterPasswordBytes(string password)
    {
        return Convert.FromBase64String(
            _hashService.HashWithMD5(_hashService.HashWithHMAC(password, "16characterslong")));
    }
}
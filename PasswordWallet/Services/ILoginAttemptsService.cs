using PasswordManager.Application.Accounts.DAOs;

namespace PasswordWallet.Services;

public interface ILoginAttemptsService
{
    Task LogLoginAttempt(Guid accountId, bool isSuccessful, string ipAddress);
    Task<DateTime?> LastSuccessfulLoginAttemptTime(Guid accountId);
    Task<DateTime?> LastUnsuccessfulLoginAttemptTime(Guid accountId);
    Task<int> ThrottleLogInTime(Guid accountId);
    Task<int> ThrottleIpLogIn(Guid accountId, string ipAddress); 
    Task<List<IpAddressBlockDao>> ListIpAddressBlocks(Guid accountId);
    Task UnlockIpAddress(Guid accountId, string ipAddress);
}
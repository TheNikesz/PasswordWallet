namespace PasswordWallet.Services
{
    public interface IUserAccessor
    {
        string? GetUserId();
        string GetRequestIpAddress();
    }
}
namespace PasswordWallet.DTOs.LoginAttempts;

public class IpAddressBlockDao
{
    public Guid AccountId { get; set; }
    public string IpAddress { get; set; }
}
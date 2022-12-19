namespace PasswordManager.Application.Accounts.DAOs;

public class IpAddressBlockDao
{
    public Guid AccountId { get; set; }
    public string IpAddress { get; set; }
}
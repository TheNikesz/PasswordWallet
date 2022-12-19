namespace PasswordWallet.Entities;

public class IpAddressBlock
{
    public Guid Id { get; set; }

    public string IpAddress { get; set; } = null!;

    public Account Account { get; set; }
    public Guid AccountId { get; set; }
}
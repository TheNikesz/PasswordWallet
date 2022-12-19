namespace PasswordWallet.Entities;

public class LoginAttempt
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public bool IsSuccessful { get; set; }
    public string IpAddress { get; set; } = null!;

    public Account Account { get; set; } = null!;
    public Guid AccountId { get; set; }
}
namespace PasswordWallet.DTOs.SharedPassword;

public class SharedPasswordDto
{
    public Guid Id { get; set; }
    public Guid SavedPasswordId { get; set; }

    public string Owner { get; set; } = null!;
    public string WebAddress { get; set; } = null!;
    public string? Description { get; set; }
    public string? Login { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.DTOs.SavedPassword;

public class AddPasswordDto
{
    [Required] public string Password { get; set; } = null!;
    [Required] public string WebAddress { get; set; } = null!;
    public string? Description { get; set; }
    public string? Login { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.DTOs.Account;

public class ChangePasswordDto
{
    [Required]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$")]
    public string OldPassword { get; set; } = null!;
    
    [Required]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$")]
    public string NewPassword { get; set; } = null!;
    
    [Required] public bool IsPasswordKeptAsHash { get; set; }
}
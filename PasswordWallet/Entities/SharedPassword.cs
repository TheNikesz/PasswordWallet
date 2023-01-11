using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Entities;

public class SharedPassword
{
    public Guid Id { get; set; }

    public Account Account { get; set; }
    [Required] public Guid AccountId { get; set; }

    public SavedPassword SavedPassword { get; set; }
    [Required] public Guid SavedPasswordId { get; set; }
}
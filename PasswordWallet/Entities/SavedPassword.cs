using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Entities;

public class SavedPassword
{
    public Guid Id { get; set; }
    [Required] [MaxLength(256)] public string Password { get; set; } = null!;
    [Required] [MaxLength(256)] public string WebAddress { get; set; } = null!;
    [MaxLength(256)] public string? Description { get; set; }
    [MaxLength(30)] public string? Login { get; set; }
    [Required] public string Iv { get; set; } = null!;

    public Account Account { get; set; } = null!;
    [Required] public Guid AccountId { get; set; }
    public ICollection<SharedPassword> SharedPasswords { get; set; } = new List<SharedPassword>();
}
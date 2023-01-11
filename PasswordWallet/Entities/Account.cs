using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Entities;

public class Account
{
    public Guid Id { get; set; }
    [Required][MaxLength(30)]
    public string Login { get; set; } = null!;
    [Required][MaxLength(512)]
    public string PasswordHash { get; set; } = null!;
    [Required][MaxLength(512)]
    public string Salt { get; set; } = null!;
    public bool IsPasswordKeptAsHash { get; set; }
    public ICollection<SavedPassword> SavedPasswords { get; set; } = new List<SavedPassword>();
    public ICollection<SharedPassword> SharedPasswords { get; set; } = new List<SharedPassword>();

}
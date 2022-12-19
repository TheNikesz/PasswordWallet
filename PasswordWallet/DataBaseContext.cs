using Microsoft.EntityFrameworkCore;
using PasswordWallet.Entities;

namespace PasswordWallet;

public class DataBaseContext : DbContext
{
    public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<SavedPassword> SavedPasswords { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<IpAddressBlock> IpAddressBlocks { get; set; }
}
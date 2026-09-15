using Microsoft.EntityFrameworkCore;

using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence;

public sealed class WalletTransferDbContext : DbContext
{
    public WalletTransferDbContext(
        DbContextOptions<WalletTransferDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WalletTransferDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
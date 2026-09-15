using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WalletTransfer.Infrastructure.Persistence;

public sealed class WalletTransferDbContextFactory
    : IDesignTimeDbContextFactory<WalletTransferDbContext>
{
    public WalletTransferDbContext CreateDbContext(
        string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;" +
               "Database=wallet_transfer;" +
               "Username=wallet_user;" +
               "Password=wallet_password";

        var optionsBuilder =
            new DbContextOptionsBuilder<WalletTransferDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new WalletTransferDbContext(
            optionsBuilder.Options);
    }
}
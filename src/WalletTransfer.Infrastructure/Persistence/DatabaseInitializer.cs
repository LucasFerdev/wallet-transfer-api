using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WalletTransfer.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<WalletTransferDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
        await DatabaseSeeder.SeedAsync(
            context,
            cancellationToken);
    }
}
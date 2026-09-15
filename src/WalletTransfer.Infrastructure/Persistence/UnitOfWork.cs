using WalletTransfer.Application.Abstractions.Persistence;

namespace WalletTransfer.Infrastructure.Persistence;

public sealed class UnitOfWork(WalletTransferDbContext context)
    : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation(cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
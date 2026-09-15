using Microsoft.EntityFrameworkCore;

using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence.Repositories;

public sealed class TransferRepository(WalletTransferDbContext context)
    : ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return context.Transfers.SingleOrDefaultAsync(
            transfer => transfer.Id == id,
            cancellationToken);
    }

    public async Task AddAsync(
        Transfer transfer,
        CancellationToken cancellationToken)
    {
        await context.Transfers.AddAsync(
            transfer,
            cancellationToken);
    }
}
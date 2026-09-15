using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Transfer transfer,
        CancellationToken cancellationToken = default);
}
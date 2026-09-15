using WalletTransfer.Application.Transfers.Models;

namespace WalletTransfer.Application.Transfers;

public interface ITransferService
{
    Task<TransferResult> ExecuteAsync(
        CreateTransferCommand command,
        CancellationToken cancellationToken = default);
}
namespace WalletTransfer.Application.Abstractions.ExternalServices;

public interface IRecipientNotifier
{
    Task<bool> NotifyAsync(
        long payeeId,
        Guid transferId,
        CancellationToken cancellationToken = default);
}
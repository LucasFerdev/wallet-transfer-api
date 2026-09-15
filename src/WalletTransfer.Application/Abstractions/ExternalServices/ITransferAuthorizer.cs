namespace WalletTransfer.Application.Abstractions.ExternalServices;

public interface ITransferAuthorizer
{
    Task<bool> AuthorizeAsync(
        CancellationToken cancellationToken = default);
}
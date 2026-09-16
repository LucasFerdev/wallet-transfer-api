using WalletTransfer.Application.Abstractions.ExternalServices;

namespace WalletTransfer.IntegrationTests.Support;

public sealed class TestTransferAuthorizer
    : ITransferAuthorizer
{
    public bool IsAuthorized { get; set; } = true;

    public Task<bool> AuthorizeAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsAuthorized);
    }
}

public sealed class TestRecipientNotifier
    : IRecipientNotifier
{
    public bool NotificationResult { get; set; } = true;

    public Task<bool> NotifyAsync(
        long payeeId,
        Guid transferId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(NotificationResult);
    }
}
using System.Net.Http.Json;

using WalletTransfer.Application.Abstractions.ExternalServices;

namespace WalletTransfer.Infrastructure.ExternalServices;

public sealed class RecipientNotifier(HttpClient httpClient)
    : IRecipientNotifier
{
    public async Task<bool> NotifyAsync(
        long payeeId,
        Guid transferId,
        CancellationToken cancellationToken = default)
    {
        var request = new NotificationRequest(
            payeeId,
            transferId);

        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/notify",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private sealed record NotificationRequest(
        long PayeeId,
        Guid TransferId);
}
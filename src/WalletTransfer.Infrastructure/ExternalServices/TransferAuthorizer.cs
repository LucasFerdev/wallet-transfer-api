using System.Net.Http.Json;

using WalletTransfer.Application.Abstractions.ExternalServices;

namespace WalletTransfer.Infrastructure.ExternalServices;

public sealed class TransferAuthorizer(HttpClient httpClient)
    : ITransferAuthorizer
{
    public async Task<bool> AuthorizeAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            "api/v2/authorize",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<AuthorizationResponse>(
                cancellationToken);

        return string.Equals(
                   result?.Status,
                   "success",
                   StringComparison.OrdinalIgnoreCase)
               && result.Data?.Authorization == true;
    }

    private sealed record AuthorizationResponse(
        string Status,
        AuthorizationData? Data);

    private sealed record AuthorizationData(
        bool Authorization);
}
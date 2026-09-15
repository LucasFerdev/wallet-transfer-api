using System.Net;

using WalletTransfer.Infrastructure.ExternalServices;

namespace WalletTransfer.UnitTests.Infrastructure.ExternalServices;

public sealed class RecipientNotifierTests
{
    [Fact]
    public async Task NotifyAsync_ShouldReturnTrue_WhenRequestSucceeds()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using var httpClient = CreateHttpClient(handler);
        var notifier = new RecipientNotifier(httpClient);
        var transferId = Guid.NewGuid();

        var result = await notifier.NotifyAsync(
            15,
            transferId);

        Assert.True(result);
        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal(
            "/api/v1/notify",
            handler.Request?.RequestUri?.AbsolutePath);

        var requestBody =
            await handler.Request!.Content!.ReadAsStringAsync();

        Assert.Contains("\"payeeId\":15", requestBody);
        Assert.Contains(transferId.ToString(), requestBody);
    }

    [Fact]
    public async Task NotifyAsync_ShouldReturnFalse_WhenRequestFails()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(
                HttpStatusCode.ServiceUnavailable));

        using var httpClient = CreateHttpClient(handler);
        var notifier = new RecipientNotifier(httpClient);

        var result = await notifier.NotifyAsync(
            15,
            Guid.NewGuid());

        Assert.False(result);
    }

    private static HttpClient CreateHttpClient(
        HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://util.devi.tools/")
        };
    }
}
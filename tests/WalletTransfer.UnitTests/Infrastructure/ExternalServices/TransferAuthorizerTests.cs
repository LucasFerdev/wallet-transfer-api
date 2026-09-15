using System.Net;
using System.Text;

using WalletTransfer.Infrastructure.ExternalServices;

namespace WalletTransfer.UnitTests.Infrastructure.ExternalServices;

public sealed class TransferAuthorizerTests
{
    [Fact]
    public async Task AuthorizeAsync_ShouldReturnTrue_WhenAuthorized()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = CreateJsonContent(
                    """
                    {
                      "status": "success",
                      "data": {
                        "authorization": true
                      }
                    }
                    """)
            });

        using var httpClient = CreateHttpClient(handler);
        var authorizer = new TransferAuthorizer(httpClient);

        var result = await authorizer.AuthorizeAsync();

        Assert.True(result);
        Assert.Equal(HttpMethod.Get, handler.Request?.Method);
        Assert.Equal(
            "/api/v2/authorize",
            handler.Request?.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldReturnFalse_WhenNotAuthorized()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = CreateJsonContent(
                    """
                    {
                      "status": "success",
                      "data": {
                        "authorization": false
                      }
                    }
                    """)
            });

        using var httpClient = CreateHttpClient(handler);
        var authorizer = new TransferAuthorizer(httpClient);

        var result = await authorizer.AuthorizeAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldThrow_WhenServiceReturnsError()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        using var httpClient = CreateHttpClient(handler);
        var authorizer = new TransferAuthorizer(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => authorizer.AuthorizeAsync());
    }

    private static HttpClient CreateHttpClient(
        HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://util.devi.tools/")
        };
    }

    private static StringContent CreateJsonContent(string json)
    {
        return new StringContent(
            json,
            Encoding.UTF8,
            "application/json");
    }
}
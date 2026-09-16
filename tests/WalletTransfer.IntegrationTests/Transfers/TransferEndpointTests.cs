using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using WalletTransfer.Domain.Enums;
using WalletTransfer.Infrastructure.Persistence;
using WalletTransfer.IntegrationTests.Support;

namespace WalletTransfer.IntegrationTests.Transfers;

public sealed class TransferEndpointTests :
    IClassFixture<WalletTransferApiFactory>,
    IAsyncLifetime
{
    private readonly WalletTransferApiFactory _factory;
    private readonly HttpClient _client;

    public TransferEndpointTests(
        WalletTransferApiFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    public async Task InitializeAsync()
    {
        _factory.Authorizer.IsAuthorized = true;
        _factory.Notifier.NotificationResult = true;

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<WalletTransferDbContext>();

        await context.Transfers.ExecuteDeleteAsync();

        await context.Wallets
            .Where(wallet => wallet.UserId == 4)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    wallet => wallet.Balance,
                    1_000m));

        await context.Wallets
            .Where(wallet => wallet.UserId == 15)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    wallet => wallet.Balance,
                    0m));
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnCreated_WhenValid()
    {
        using var response = await PostTransferAsync(
            100m,
            4,
            15);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal(100m, root.GetProperty("value").GetDecimal());
        Assert.Equal(4, root.GetProperty("payer").GetInt64());
        Assert.Equal(15, root.GetProperty("payee").GetInt64());
        Assert.Equal(
            "Completed",
            root.GetProperty("status").GetString());
        Assert.True(
            root.GetProperty("notificationSent").GetBoolean());

        var (payerBalance, payeeBalance) =
            await GetBalancesAsync();

        Assert.Equal(900m, payerBalance);
        Assert.Equal(100m, payeeBalance);
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnBadRequest_WhenUsersAreEqual()
    {
        using var response = await PostTransferAsync(
            100m,
            4,
            4);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnBadRequest_WhenValueIsInvalid()
    {
        using var response = await PostTransferAsync(
            0m,
            4,
            15);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnUnprocessableEntity_WhenPayerIsMerchant()
    {
        using var response = await PostTransferAsync(
            100m,
            15,
            4);

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        using var response = await PostTransferAsync(
            100m,
            999,
            15);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTransfer_ShouldReturnForbidden_WhenNotAuthorized()
    {
        _factory.Authorizer.IsAuthorized = false;

        using var response = await PostTransferAsync(
            100m,
            4,
            15);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var (payerBalance, payeeBalance) =
            await GetBalancesAsync();

        Assert.Equal(1_000m, payerBalance);
        Assert.Equal(0m, payeeBalance);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<WalletTransferDbContext>();

        var transfer = await context.Transfers.SingleAsync();

        Assert.Equal(TransferStatus.Failed, transfer.Status);
    }

    [Fact]
    public async Task PostTransfer_ShouldRemainCreated_WhenNotificationFails()
    {
        _factory.Notifier.NotificationResult = false;

        using var response = await PostTransferAsync(
            100m,
            4,
            15);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.False(
            document.RootElement
                .GetProperty("notificationSent")
                .GetBoolean());

        var (payerBalance, payeeBalance) =
            await GetBalancesAsync();

        Assert.Equal(900m, payerBalance);
        Assert.Equal(100m, payeeBalance);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    private Task<HttpResponseMessage> PostTransferAsync(
        decimal value,
        long payer,
        long payee)
    {
        return _client.PostAsJsonAsync(
            "/transfer",
            new
            {
                value,
                payer,
                payee
            });
    }

    private async Task<(decimal Payer, decimal Payee)>
        GetBalancesAsync()
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<WalletTransferDbContext>();

        var payerBalance = await context.Wallets
            .Where(wallet => wallet.UserId == 4)
            .Select(wallet => wallet.Balance)
            .SingleAsync();

        var payeeBalance = await context.Wallets
            .Where(wallet => wallet.UserId == 15)
            .Select(wallet => wallet.Balance)
            .SingleAsync();

        return (payerBalance, payeeBalance);
    }
}
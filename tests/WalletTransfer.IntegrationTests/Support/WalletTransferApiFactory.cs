using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using WalletTransfer.Application.Abstractions.ExternalServices;

namespace WalletTransfer.IntegrationTests.Support;

public sealed class WalletTransferApiFactory
    : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;" +
        "Database=wallet_transfer_tests;" +
        "Username=wallet_user;" +
        "Password=wallet_password";

    public TestTransferAuthorizer Authorizer { get; } = new();
    public TestRecipientNotifier Notifier { get; } = new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "ConnectionStrings:Database",
            TestConnectionString);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITransferAuthorizer>();
            services.RemoveAll<IRecipientNotifier>();

            services.AddSingleton<ITransferAuthorizer>(
                Authorizer);

            services.AddSingleton<IRecipientNotifier>(
                Notifier);
        });
    }
}
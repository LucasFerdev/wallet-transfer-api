using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using WalletTransfer.Application.Abstractions.ExternalServices;
using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Infrastructure.ExternalServices;
using WalletTransfer.Infrastructure.Persistence;
using WalletTransfer.Infrastructure.Persistence.Repositories;

namespace WalletTransfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string externalServicesBaseUrl)
    {
        if (!Uri.TryCreate(
                externalServicesBaseUrl,
                UriKind.Absolute,
                out var baseAddress))
        {
            throw new InvalidOperationException(
                "A URL dos serviços externos é inválida.");
        }

        services.AddDbContext<WalletTransferDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<ITransferAuthorizer, TransferAuthorizer>(
            client =>
            {
                client.BaseAddress = baseAddress;
                client.Timeout = TimeSpan.FromSeconds(5);
            });

        services.AddHttpClient<IRecipientNotifier, RecipientNotifier>(
            client =>
            {
                client.BaseAddress = baseAddress;
                client.Timeout = TimeSpan.FromSeconds(5);
            });

        return services;
    }
}
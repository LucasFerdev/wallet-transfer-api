using Microsoft.Extensions.DependencyInjection;

using WalletTransfer.Application.Transfers;

namespace WalletTransfer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ITransferService, TransferService>();

        return services;
    }
}
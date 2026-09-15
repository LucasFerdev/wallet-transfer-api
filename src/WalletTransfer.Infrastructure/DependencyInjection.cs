using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Infrastructure.Persistence;
using WalletTransfer.Infrastructure.Persistence.Repositories;

namespace WalletTransfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<WalletTransferDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
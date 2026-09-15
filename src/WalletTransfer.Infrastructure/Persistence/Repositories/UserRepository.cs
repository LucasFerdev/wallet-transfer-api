using Microsoft.EntityFrameworkCore;

using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(WalletTransferDbContext context)
    : IUserRepository
{
    public Task<User?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return context.Users
            .Include(user => user.Wallet)
            .SingleOrDefaultAsync(
                user => user.Id == id,
                cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return context.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken);
    }

    public Task<bool> DocumentExistsAsync(
        string document,
        CancellationToken cancellationToken)
    {
        return context.Users.AnyAsync(
            user => user.Document == document,
            cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }
}
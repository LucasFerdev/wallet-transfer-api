using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Enums;

namespace WalletTransfer.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        WalletTransferDbContext context,
        CancellationToken cancellationToken = default)
    {
        var existingIds = await context.Users
            .Where(user => user.Id == 4 || user.Id == 15)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        if (!existingIds.Contains(4))
        {
            AddUser(
                context,
                4,
                new User(
                    "Usuário Comum",
                    "12345678901",
                    "usuario@wallet.local",
                    HashPassword("Wallet@123"),
                    UserType.Common,
                    1_000m));
        }

        if (!existingIds.Contains(15))
        {
            AddUser(
                context,
                15,
                new User(
                    "Lojista",
                    "12345678000199",
                    "lojista@wallet.local",
                    HashPassword("Wallet@123"),
                    UserType.Merchant,
                    0m));
        }

        await context.SaveChangesAsync(cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            SELECT setval(
                pg_get_serial_sequence('users', 'id'),
                GREATEST(
                    (SELECT COALESCE(MAX(id), 1) FROM users),
                    1
                ),
                true
            );
            """,
            cancellationToken);
    }

    private static void AddUser(
        WalletTransferDbContext context,
        long id,
        User user)
    {
        context.Entry(user)
            .Property(nameof(User.Id))
            .CurrentValue = id;

        context.Users.Add(user);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return string.Join(
            '$',
            "PBKDF2-SHA256",
            "100000",
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }
}
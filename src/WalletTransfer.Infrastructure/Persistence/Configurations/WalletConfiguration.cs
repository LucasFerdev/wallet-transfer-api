using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence.Configurations;

public sealed class WalletConfiguration
    : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(wallet => wallet.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(wallet => wallet.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(wallet => wallet.Version)
            .IsRowVersion();

        builder.HasIndex(wallet => wallet.UserId)
            .IsUnique()
            .HasDatabaseName("ux_wallets_user_id");
    }
}
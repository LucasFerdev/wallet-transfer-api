using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Document)
            .HasColumnName("document")
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(user => user.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(user => user.Document)
            .IsUnique()
            .HasDatabaseName("ux_users_document");

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email");

        builder.HasOne(user => user.Wallet)
            .WithOne()
            .HasForeignKey<Wallet>(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Wallet)
            .IsRequired();
    }
}
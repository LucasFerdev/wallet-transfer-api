using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration
    : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers");

        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(transfer => transfer.PayerId)
            .HasColumnName("payer_id")
            .IsRequired();

        builder.Property(transfer => transfer.PayeeId)
            .HasColumnName("payee_id")
            .IsRequired();

        builder.Property(transfer => transfer.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transfer => transfer.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(transfer => transfer.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(transfer => transfer.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(transfer => transfer.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(transfer => transfer.PayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(transfer => transfer.PayeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => new
        {
            transfer.PayerId,
            transfer.CreatedAt
        })
            .HasDatabaseName("ix_transfers_payer_created_at");

        builder.HasIndex(transfer => new
        {
            transfer.PayeeId,
            transfer.CreatedAt
        })
            .HasDatabaseName("ix_transfers_payee_created_at");
    }
}
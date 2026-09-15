using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Enums;

namespace WalletTransfer.Application.Transfers.Models;

public sealed record TransferResult(
    Guid Id,
    decimal Value,
    long Payer,
    long Payee,
    TransferStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    bool NotificationSent)
{
    public static TransferResult FromEntity(
        Transfer transfer,
        bool notificationSent)
    {
        return new TransferResult(
            transfer.Id,
            transfer.Amount,
            transfer.PayerId,
            transfer.PayeeId,
            transfer.Status,
            transfer.CreatedAt,
            transfer.CompletedAt,
            notificationSent);
    }
}
using WalletTransfer.Domain.Enums;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.Domain.Entities;

public sealed class Transfer
{
    public Guid Id { get; private set; }
    public long PayerId { get; private set; }
    public long PayeeId { get; private set; }
    public decimal Amount { get; private set; }
    public TransferStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Transfer()
    {
    }

    public Transfer(
        long payerId,
        long payeeId,
        decimal amount)
    {
        ValidateParticipants(payerId, payeeId);
        ValidateAmount(amount);

        Id = Guid.NewGuid();
        PayerId = payerId;
        PayeeId = payeeId;
        Amount = amount;
        Status = TransferStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        EnsurePending();

        Status = TransferStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        FailureReason = null;
    }

    public void Fail(string reason)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException(
                "O motivo da falha é obrigatório.");
        }

        Status = TransferStatus.Failed;
        FailureReason = reason.Trim();
        CompletedAt = null;
    }

    private void EnsurePending()
    {
        if (Status != TransferStatus.Pending)
        {
            throw new DomainException(
                "Somente transferências pendentes podem ser alteradas.");
        }
    }

    private static void ValidateParticipants(
        long payerId,
        long payeeId)
    {
        if (payerId <= 0)
        {
            throw new DomainException(
                "O identificador do pagador é inválido.");
        }

        if (payeeId <= 0)
        {
            throw new DomainException(
                "O identificador do recebedor é inválido.");
        }

        if (payerId == payeeId)
        {
            throw new DomainException(
                "O pagador e o recebedor devem ser diferentes.");
        }
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException(
                "O valor da transferência deve ser maior que zero.");
        }
    }
}
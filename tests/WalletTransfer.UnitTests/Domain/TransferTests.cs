using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Enums;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.UnitTests.Domain;

public sealed class TransferTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingTransfer_WhenDataIsValid()
    {
        var transfer = new Transfer(1, 2, 100m);

        Assert.NotEqual(Guid.Empty, transfer.Id);
        Assert.Equal(1, transfer.PayerId);
        Assert.Equal(2, transfer.PayeeId);
        Assert.Equal(100m, transfer.Amount);
        Assert.Equal(TransferStatus.Pending, transfer.Status);
        Assert.Null(transfer.CompletedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_ShouldThrow_WhenAmountIsInvalid(
        decimal invalidAmount)
    {
        Assert.Throws<DomainException>(
            () => new Transfer(1, 2, invalidAmount));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    public void Constructor_ShouldThrow_WhenParticipantIdIsInvalid(
        long payerId,
        long payeeId)
    {
        Assert.Throws<DomainException>(
            () => new Transfer(payerId, payeeId, 100m));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPayerAndPayeeAreEqual()
    {
        Assert.Throws<DomainException>(
            () => new Transfer(1, 1, 100m));
    }

    [Fact]
    public void Complete_ShouldCompletePendingTransfer()
    {
        var transfer = new Transfer(1, 2, 100m);

        transfer.Complete();

        Assert.Equal(TransferStatus.Completed, transfer.Status);
        Assert.NotNull(transfer.CompletedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenTransferIsAlreadyCompleted()
    {
        var transfer = new Transfer(1, 2, 100m);
        transfer.Complete();

        var exception = Assert.Throws<DomainException>(
            () => transfer.Complete());

        Assert.Equal(
            "Somente transferências pendentes podem ser alteradas.",
            exception.Message);
    }

    [Fact]
    public void Fail_ShouldMarkPendingTransferAsFailed()
    {
        var transfer = new Transfer(1, 2, 100m);

        transfer.Fail("Serviço autorizador indisponível.");

        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.Equal(
            "Serviço autorizador indisponível.",
            transfer.FailureReason);
        Assert.Null(transfer.CompletedAt);
    }

    [Fact]
    public void Fail_ShouldThrow_WhenReasonIsEmpty()
    {
        var transfer = new Transfer(1, 2, 100m);

        var exception = Assert.Throws<DomainException>(
            () => transfer.Fail(" "));

        Assert.Equal(
            "O motivo da falha é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void Fail_ShouldThrow_WhenTransferIsAlreadyCompleted()
    {
        var transfer = new Transfer(1, 2, 100m);
        transfer.Complete();

        Assert.Throws<DomainException>(
            () => transfer.Fail("Falha tardia."));
    }
}
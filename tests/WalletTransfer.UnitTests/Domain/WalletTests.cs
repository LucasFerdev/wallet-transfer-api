using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.UnitTests.Domain;

public sealed class WalletTests
{
    [Fact]
    public void Constructor_ShouldCreateWallet_WhenInitialBalanceIsValid()
    {
        var wallet = new Wallet(100m);

        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInitialBalanceIsNegative()
    {
        var action = () => new Wallet(-1m);

        var exception = Assert.Throws<DomainException>(action);

        Assert.Equal(
            "O saldo inicial não pode ser negativo.",
            exception.Message);
    }

    [Fact]
    public void Debit_ShouldSubtractAmount_WhenBalanceIsSufficient()
    {
        var wallet = new Wallet(100m);

        wallet.Debit(40m);

        Assert.Equal(60m, wallet.Balance);
    }

    [Fact]
    public void Debit_ShouldThrow_WhenBalanceIsInsufficient()
    {
        var wallet = new Wallet(50m);

        var action = () => wallet.Debit(100m);

        var exception = Assert.Throws<DomainException>(action);

        Assert.Equal(
            "O usuário não possui saldo suficiente.",
            exception.Message);
    }

    [Fact]
    public void Credit_ShouldAddAmount_WhenAmountIsValid()
    {
        var wallet = new Wallet(100m);

        wallet.Credit(25m);

        Assert.Equal(125m, wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Operations_ShouldThrow_WhenAmountIsNotPositive(
        decimal invalidAmount)
    {
        var wallet = new Wallet(100m);

        Assert.Throws<DomainException>(
            () => wallet.Debit(invalidAmount));

        Assert.Throws<DomainException>(
            () => wallet.Credit(invalidAmount));
    }
}
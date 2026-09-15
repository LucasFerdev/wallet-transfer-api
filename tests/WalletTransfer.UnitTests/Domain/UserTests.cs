using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Enums;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.UnitTests.Domain;

public sealed class UserTests
{
    private const string PasswordHash = "senha-protegida-para-teste";

    [Fact]
    public void Constructor_ShouldCreateCommonUser_WhenDataIsValid()
    {
        var user = new User(
            "Lucas Silva",
            "123.456.789-00",
            "LUCAS@EMAIL.COM",
            PasswordHash,
            UserType.Common,
            100m);

        Assert.Equal("Lucas Silva", user.FullName);
        Assert.Equal("12345678900", user.Document);
        Assert.Equal("lucas@email.com", user.Email);
        Assert.Equal(UserType.Common, user.Type);
        Assert.Equal(100m, user.Wallet.Balance);
        Assert.True(user.CanTransfer);
    }

    [Fact]
    public void Constructor_ShouldCreateMerchant_WhenDataIsValid()
    {
        var merchant = new User(
            "Loja Exemplo",
            "12.345.678/0001-99",
            "LOJA@EMAIL.COM",
            PasswordHash,
            UserType.Merchant);

        Assert.Equal("12345678000199", merchant.Document);
        Assert.Equal("loja@email.com", merchant.Email);
        Assert.Equal(UserType.Merchant, merchant.Type);
        Assert.False(merchant.CanTransfer);
    }

    [Fact]
    public void EnsureCanTransfer_ShouldNotThrow_WhenUserIsCommon()
    {
        var user = CreateCommonUser();

        var exception = Record.Exception(
            () => user.EnsureCanTransfer());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanTransfer_ShouldThrow_WhenUserIsMerchant()
    {
        var merchant = CreateMerchant();

        var exception = Assert.Throws<DomainException>(
            () => merchant.EnsureCanTransfer());

        Assert.Equal(
            "Lojistas não podem realizar transferências.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFullNameIsEmpty()
    {
        Assert.Throws<DomainException>(() => new User(
            "",
            "12345678900",
            "usuario@email.com",
            PasswordHash,
            UserType.Common));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEmailIsInvalid()
    {
        Assert.Throws<DomainException>(() => new User(
            "Lucas Silva",
            "12345678900",
            "email-invalido",
            PasswordHash,
            UserType.Common));
    }

    [Theory]
    [InlineData("123456789", UserType.Common)]
    [InlineData("12345678900", UserType.Merchant)]
    public void Constructor_ShouldThrow_WhenDocumentLengthIsInvalid(
        string document,
        UserType type)
    {
        Assert.Throws<DomainException>(() => new User(
            "Usuário Teste",
            document,
            "usuario@email.com",
            PasswordHash,
            type));
    }

    private static User CreateCommonUser()
    {
        return new User(
            "Usuário Comum",
            "12345678900",
            "usuario@email.com",
            PasswordHash,
            UserType.Common);
    }

    private static User CreateMerchant()
    {
        return new User(
            "Lojista Teste",
            "12345678000199",
            "lojista@email.com",
            PasswordHash,
            UserType.Merchant);
    }
}
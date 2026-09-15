using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.Domain.Entities;

public sealed class Wallet
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public decimal Balance { get; private set; }
    public uint Version { get; private set; }

    private Wallet()
    {
    }

    public Wallet(decimal initialBalance = 0)
    {
        if (initialBalance < 0)
        {
            throw new DomainException(
                "O saldo inicial não pode ser negativo.");
        }

        Balance = initialBalance;
    }

    public void Debit(decimal amount)
    {
        EnsureCanDebit(amount);
        Balance -= amount;
    }

    public void EnsureCanDebit(decimal amount)
    {
        ValidateAmount(amount);

        if (Balance < amount)
        {
            throw new DomainException(
                "O usuário não possui saldo suficiente.");
        }
    }

    public void Credit(decimal amount)
    {
        ValidateAmount(amount);
        Balance += amount;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException(
                "O valor deve ser maior que zero.");
        }
    }
}
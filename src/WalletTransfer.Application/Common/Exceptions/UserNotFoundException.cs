namespace WalletTransfer.Application.Common.Exceptions;

public sealed class UserNotFoundException : Exception
{
    public long UserId { get; }

    public UserNotFoundException(long userId)
        : base($"O usuário com identificador {userId} não foi encontrado.")
    {
        UserId = userId;
    }
}
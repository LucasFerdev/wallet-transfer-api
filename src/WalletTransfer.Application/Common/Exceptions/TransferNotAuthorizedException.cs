namespace WalletTransfer.Application.Common.Exceptions;

public sealed class TransferNotAuthorizedException : Exception
{
    public TransferNotAuthorizedException()
        : base("A transferência não foi autorizada.")
    {
    }
}
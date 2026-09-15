namespace WalletTransfer.Application.Transfers.Models;

public sealed record CreateTransferCommand(
    decimal Value,
    long Payer,
    long Payee);
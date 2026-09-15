using WalletTransfer.Application.Abstractions.ExternalServices;
using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Application.Common.Exceptions;
using WalletTransfer.Application.Transfers.Models;
using WalletTransfer.Domain.Entities;

namespace WalletTransfer.Application.Transfers;

public sealed class TransferService : ITransferService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransferAuthorizer _authorizer;
    private readonly IRecipientNotifier _notifier;

    public TransferService(
        IUserRepository userRepository,
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        ITransferAuthorizer authorizer,
        IRecipientNotifier notifier)
    {
        _userRepository = userRepository;
        _transferRepository = transferRepository;
        _unitOfWork = unitOfWork;
        _authorizer = authorizer;
        _notifier = notifier;
    }

    public async Task<TransferResult> ExecuteAsync(
        CreateTransferCommand command,
        CancellationToken cancellationToken = default)
    {
        var transfer = new Transfer(
            command.Payer,
            command.Payee,
            command.Value);

        var payer = await _userRepository.GetByIdAsync(
            command.Payer,
            cancellationToken)
            ?? throw new UserNotFoundException(command.Payer);

        var payee = await _userRepository.GetByIdAsync(
            command.Payee,
            cancellationToken)
            ?? throw new UserNotFoundException(command.Payee);

        payer.EnsureCanTransfer();
        payer.Wallet.EnsureCanDebit(command.Value);

        var isAuthorized = await _authorizer.AuthorizeAsync(
            cancellationToken);

        if (!isAuthorized)
        {
            transfer.Fail("Transferência recusada pelo autorizador externo.");

            await _unitOfWork.ExecuteInTransactionAsync(
                async transactionToken =>
                {
                    await _transferRepository.AddAsync(
                        transfer,
                        transactionToken);
                },
                cancellationToken);

            throw new TransferNotAuthorizedException();
        }

        await _unitOfWork.ExecuteInTransactionAsync(
            async transactionToken =>
            {
                payer.Wallet.Debit(command.Value);
                payee.Wallet.Credit(command.Value);
                transfer.Complete();

                await _transferRepository.AddAsync(
                    transfer,
                    transactionToken);
            },
            cancellationToken);

        var notificationSent = await TryNotifyAsync(
            transfer,
            cancellationToken);

        return TransferResult.FromEntity(
            transfer,
            notificationSent);
    }

    private async Task<bool> TryNotifyAsync(
        Transfer transfer,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _notifier.NotifyAsync(
                transfer.PayeeId,
                transfer.Id,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
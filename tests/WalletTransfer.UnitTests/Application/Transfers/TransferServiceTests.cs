using WalletTransfer.Application.Abstractions.ExternalServices;
using WalletTransfer.Application.Abstractions.Persistence;
using WalletTransfer.Application.Common.Exceptions;
using WalletTransfer.Application.Transfers;
using WalletTransfer.Application.Transfers.Models;
using WalletTransfer.Domain.Entities;
using WalletTransfer.Domain.Enums;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.UnitTests.Application.Transfers;

public sealed class TransferServiceTests
{
    private const string PasswordHash = "senha-protegida-para-teste";

    [Fact]
    public async Task ExecuteAsync_ShouldTransferMoney_WhenRequestIsValid()
    {
        var payer = CreateCommonUser(
            "pagador@email.com",
            100m);

        var payee = CreateCommonUser(
            "recebedor@email.com",
            20m);

        var context = CreateContext(new Dictionary<long, User>
        {
            [1] = payer,
            [2] = payee
        });

        var result = await context.Service.ExecuteAsync(
            new CreateTransferCommand(40m, 1, 2));

        Assert.Equal(60m, payer.Wallet.Balance);
        Assert.Equal(60m, payee.Wallet.Balance);
        Assert.Equal(TransferStatus.Completed, result.Status);
        Assert.True(result.NotificationSent);
        Assert.Single(context.TransferRepository.Transfers);
        Assert.Equal(1, context.Authorizer.CallCount);
        Assert.Equal(1, context.Notifier.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenPayerIsMerchant()
    {
        var payer = CreateMerchant(100m);
        var payee = CreateCommonUser(
            "recebedor@email.com",
            20m);

        var context = CreateContext(new Dictionary<long, User>
        {
            [1] = payer,
            [2] = payee
        });

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => context.Service.ExecuteAsync(
                new CreateTransferCommand(40m, 1, 2)));

        Assert.Equal(
            "Lojistas não podem realizar transferências.",
            exception.Message);

        Assert.Equal(100m, payer.Wallet.Balance);
        Assert.Equal(20m, payee.Wallet.Balance);
        Assert.Empty(context.TransferRepository.Transfers);
        Assert.Equal(0, context.Authorizer.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenBalanceIsInsufficient()
    {
        var payer = CreateCommonUser(
            "pagador@email.com",
            30m);

        var payee = CreateCommonUser(
            "recebedor@email.com",
            20m);

        var context = CreateContext(new Dictionary<long, User>
        {
            [1] = payer,
            [2] = payee
        });

        await Assert.ThrowsAsync<DomainException>(
            () => context.Service.ExecuteAsync(
                new CreateTransferCommand(40m, 1, 2)));

        Assert.Equal(30m, payer.Wallet.Balance);
        Assert.Equal(20m, payee.Wallet.Balance);
        Assert.Empty(context.TransferRepository.Transfers);
        Assert.Equal(0, context.Authorizer.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ShouldThrow_WhenParticipantDoesNotExist(
        bool payerIsMissing)
    {
        var users = new Dictionary<long, User>();

        if (payerIsMissing)
        {
            users[2] = CreateCommonUser(
                "recebedor@email.com",
                20m);
        }
        else
        {
            users[1] = CreateCommonUser(
                "pagador@email.com",
                100m);
        }

        var context = CreateContext(users);

        var exception = await Assert.ThrowsAsync<UserNotFoundException>(
            () => context.Service.ExecuteAsync(
                new CreateTransferCommand(40m, 1, 2)));

        var expectedMissingId = payerIsMissing ? 1 : 2;

        Assert.Equal(expectedMissingId, exception.UserId);
        Assert.Empty(context.TransferRepository.Transfers);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordFailure_WhenNotAuthorized()
    {
        var payer = CreateCommonUser(
            "pagador@email.com",
            100m);

        var payee = CreateCommonUser(
            "recebedor@email.com",
            20m);

        var context = CreateContext(
            new Dictionary<long, User>
            {
                [1] = payer,
                [2] = payee
            },
            isAuthorized: false);

        await Assert.ThrowsAsync<TransferNotAuthorizedException>(
            () => context.Service.ExecuteAsync(
                new CreateTransferCommand(40m, 1, 2)));

        Assert.Equal(100m, payer.Wallet.Balance);
        Assert.Equal(20m, payee.Wallet.Balance);
        Assert.Equal(0, context.Notifier.CallCount);

        var transfer = Assert.Single(
            context.TransferRepository.Transfers);

        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.NotNull(transfer.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepTransfer_WhenNotificationFails()
    {
        var payer = CreateCommonUser(
            "pagador@email.com",
            100m);

        var payee = CreateCommonUser(
            "recebedor@email.com",
            20m);

        var context = CreateContext(
            new Dictionary<long, User>
            {
                [1] = payer,
                [2] = payee
            },
            notificationSucceeds: false);

        var result = await context.Service.ExecuteAsync(
            new CreateTransferCommand(40m, 1, 2));

        Assert.Equal(TransferStatus.Completed, result.Status);
        Assert.False(result.NotificationSent);
        Assert.Equal(60m, payer.Wallet.Balance);
        Assert.Equal(60m, payee.Wallet.Balance);
        Assert.Single(context.TransferRepository.Transfers);
    }

    private static TestContext CreateContext(
        Dictionary<long, User> users,
        bool isAuthorized = true,
        bool notificationSucceeds = true)
    {
        var userRepository = new FakeUserRepository(users);
        var transferRepository = new FakeTransferRepository();
        var unitOfWork = new FakeUnitOfWork();
        var authorizer = new FakeTransferAuthorizer(isAuthorized);
        var notifier = new FakeRecipientNotifier(
            notificationSucceeds);

        var service = new TransferService(
            userRepository,
            transferRepository,
            unitOfWork,
            authorizer,
            notifier);

        return new TestContext(
            service,
            transferRepository,
            authorizer,
            notifier);
    }

    private static User CreateCommonUser(
        string email,
        decimal balance)
    {
        return new User(
            "Usuário Comum",
            "12345678900",
            email,
            PasswordHash,
            UserType.Common,
            balance);
    }

    private static User CreateMerchant(decimal balance)
    {
        return new User(
            "Lojista Teste",
            "12345678000199",
            "lojista@email.com",
            PasswordHash,
            UserType.Merchant,
            balance);
    }

    private sealed record TestContext(
        TransferService Service,
        FakeTransferRepository TransferRepository,
        FakeTransferAuthorizer Authorizer,
        FakeRecipientNotifier Notifier);

    private sealed class FakeUserRepository
        : IUserRepository
    {
        private readonly Dictionary<long, User> _users;

        public FakeUserRepository(
            Dictionary<long, User> users)
        {
            _users = users;
        }

        public Task<User?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<bool> EmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> DocumentExistsAsync(
            string document,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTransferRepository
        : ITransferRepository
    {
        public List<Transfer> Transfers { get; } = [];

        public Task<Transfer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var transfer = Transfers.FirstOrDefault(
                item => item.Id == id);

            return Task.FromResult(transfer);
        }

        public Task AddAsync(
            Transfer transfer,
            CancellationToken cancellationToken = default)
        {
            Transfers.Add(transfer);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork
        : IUnitOfWork
    {
        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }

    private sealed class FakeTransferAuthorizer
        : ITransferAuthorizer
    {
        private readonly bool _isAuthorized;

        public int CallCount { get; private set; }

        public FakeTransferAuthorizer(bool isAuthorized)
        {
            _isAuthorized = isAuthorized;
        }

        public Task<bool> AuthorizeAsync(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_isAuthorized);
        }
    }

    private sealed class FakeRecipientNotifier
        : IRecipientNotifier
    {
        private readonly bool _succeeds;

        public int CallCount { get; private set; }

        public FakeRecipientNotifier(bool succeeds)
        {
            _succeeds = succeeds;
        }

        public Task<bool> NotifyAsync(
            long payeeId,
            Guid transferId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_succeeds);
        }
    }
}
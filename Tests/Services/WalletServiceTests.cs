using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class WalletServiceTests
{
    private readonly Mock<IRepository<Wallet>> _walletRepoMock;
    private readonly Mock<IRepository<Transaction>> _transactionRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly WalletService _walletService;

    public WalletServiceTests()
    {
        _walletRepoMock = new Mock<IRepository<Wallet>>();
        _transactionRepoMock = new Mock<IRepository<Transaction>>();
        _userRepoMock = new Mock<IUserRepository>();
        _walletService = new WalletService(
            _walletRepoMock.Object, _transactionRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task AddFundsAsync_ShouldAddFunds_WhenNewTransaction()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 100m };
        _transactionRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
            .ReturnsAsync((Transaction?)null);
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act
        var result = await _walletService.AddFundsAsync(1, 500m, "key_123");

        // Assert
        Assert.Equal(600m, result.Balance);
        _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task AddFundsAsync_ShouldBeIdempotent_WhenDuplicateKey()
    {
        // Arrange
        var existingTx = new Transaction { TransactionId = 1, ReferenceId = "key_123" };
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 600m };

        _transactionRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
            .ReturnsAsync(existingTx);
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act
        var result = await _walletService.AddFundsAsync(1, 500m, "key_123");

        // Assert - should return current balance without adding again
        Assert.Equal(600m, result.Balance);
        _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddFundsAsync_ShouldThrow_WhenAmountIsZero()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _walletService.AddFundsAsync(1, 0m, "key_123"));
    }

    [Fact]
    public async Task AddFundsAsync_ShouldThrow_WhenAmountIsNegative()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _walletService.AddFundsAsync(1, -100m, "key_123"));
    }

    [Fact]
    public async Task AddFundsAsync_ShouldCreateWallet_WhenNotExists()
    {
        // Arrange
        _transactionRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
            .ReturnsAsync((Transaction?)null);
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet>());
        _walletRepoMock.Setup(r => r.AddAsync(It.IsAny<Wallet>()))
            .ReturnsAsync((Wallet w) =>
            {
                w.WalletId = 1;
                return w;
            });

        // Act
        var result = await _walletService.AddFundsAsync(1, 500m, "key_new");

        // Assert
        Assert.Equal(500m, result.Balance);
        _walletRepoMock.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Fact]
    public async Task GetWalletByUserIdAsync_ShouldReturnWallet_WhenExists()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 250m };
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act
        var result = await _walletService.GetWalletByUserIdAsync(1);

        // Assert
        Assert.Equal(250m, result.Balance);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task GetWalletByUserIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _walletService.GetWalletByUserIdAsync(999));
    }

    [Fact]
    public async Task CreditWalletAsync_ShouldIncreaseBalance()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 100m };
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act
        var result = await _walletService.CreditWalletAsync(1, 200m, "Refund", "ref_1");

        // Assert
        Assert.Equal(300m, result.Balance);
        _transactionRepoMock.Verify(r => r.AddAsync(It.Is<Transaction>(
            t => t.Type == TransactionType.Credit && t.Amount == 200m)), Times.Once);
    }

    [Fact]
    public async Task DebitWalletAsync_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 500m };
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act
        var result = await _walletService.DebitWalletAsync(1, 200m, "Booking payment");

        // Assert
        Assert.Equal(300m, result.Balance);
        _transactionRepoMock.Verify(r => r.AddAsync(It.Is<Transaction>(
            t => t.Type == TransactionType.Debit && t.Amount == 200m)), Times.Once);
    }

    [Fact]
    public async Task DebitWalletAsync_ShouldThrow_WhenInsufficientBalance()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 100m };
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _walletService.DebitWalletAsync(1, 500m, "Booking payment"));
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var wallet = new Wallet { WalletId = 1, UserId = 1, Balance = 100m };
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet> { wallet });

        var transactions = Enumerable.Range(1, 25).Select(i => new Transaction
        {
            TransactionId = i,
            WalletId = 1,
            Type = TransactionType.Credit,
            Amount = 100m,
            BalanceAfter = i * 100m,
            Description = $"Transaction {i}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        _transactionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _walletService.GetTransactionHistoryAsync(1, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(10, result.Count());
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldThrow_WhenWalletNotFound()
    {
        // Arrange
        _walletRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(new List<Wallet>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _walletService.GetTransactionHistoryAsync(999));
    }
}

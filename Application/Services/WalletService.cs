using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class WalletService : IWalletService
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IUserRepository _userRepository;

    public WalletService(
        IRepository<Wallet> walletRepository,
        IRepository<Transaction> transactionRepository,
        IUserRepository userRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
    }

    public async Task<WalletResponse> AddFundsAsync(int userId, decimal amount, string idempotencyKey)
    {
        if (amount <= 0)
            throw new BusinessException("Amount must be greater than zero");

        // Check for idempotency - if transaction with this ReferenceId exists, return existing result
        var existingTransaction = await _transactionRepository.FirstOrDefaultAsync(
            t => t.ReferenceId == idempotencyKey);

        if (existingTransaction != null)
        {
            // Idempotent response - return current wallet state
            return await GetWalletByUserIdAsync(userId);
        }

        // Get or create wallet
        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
        {
            // Create wallet if doesn't exist (shouldn't happen, but safety check)
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();
        }

        // Credit wallet
        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);

        // Create transaction record
        var transaction = new Transaction
        {
            WalletId = wallet.WalletId,
            Type = TransactionType.Credit,
            Amount = amount,
            BalanceAfter = wallet.Balance,
            Description = $"Funds added via payment gateway",
            ReferenceId = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        return MapToWalletResponse(wallet);
    }

    public async Task<WalletResponse> GetWalletByUserIdAsync(int userId)
    {
        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
            throw new NotFoundException("Wallet not found for user");

        return MapToWalletResponse(wallet);
    }

    public async Task<IEnumerable<WalletTransactionResponse>> GetTransactionHistoryAsync(
        int userId, 
        int page = 1, 
        int pageSize = 20)
    {
        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
            throw new NotFoundException("Wallet not found for user");

        var transactions = await _transactionRepository.FindAsync(
            t => t.WalletId == wallet.WalletId);

        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToTransactionResponse);
    }

    public async Task<WalletResponse> CreditWalletAsync(
        int userId, 
        decimal amount, 
        string description, 
        string? referenceId = null)
    {
        if (amount <= 0)
            throw new BusinessException("Amount must be greater than zero");

        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
            throw new NotFoundException("Wallet not found for user");

        // Credit wallet
        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);

        // Create transaction record
        var transaction = new Transaction
        {
            WalletId = wallet.WalletId,
            Type = TransactionType.Credit,
            Amount = amount,
            BalanceAfter = wallet.Balance,
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        return MapToWalletResponse(wallet);
    }

    public async Task<WalletResponse> DebitWalletAsync(
        int userId, 
        decimal amount, 
        string description, 
        string? referenceId = null)
    {
        if (amount <= 0)
            throw new BusinessException("Amount must be greater than zero");

        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
            throw new NotFoundException("Wallet not found for user");

        if (wallet.Balance < amount)
            throw new BusinessException("Insufficient wallet balance");

        // Debit wallet
        wallet.Balance -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);

        // Create transaction record
        var transaction = new Transaction
        {
            WalletId = wallet.WalletId,
            Type = TransactionType.Debit,
            Amount = amount,
            BalanceAfter = wallet.Balance,
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        return MapToWalletResponse(wallet);
    }

    private WalletResponse MapToWalletResponse(Wallet wallet)
    {
        return new WalletResponse
        {
            WalletId = wallet.WalletId,
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    private WalletTransactionResponse MapToTransactionResponse(Transaction transaction)
    {
        return new WalletTransactionResponse
        {
            TransactionId = transaction.TransactionId,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            BalanceAfter = transaction.BalanceAfter,
            Description = transaction.Description,
            ReferenceId = transaction.ReferenceId,
            BookingId = transaction.BookingId,
            CreatedAt = transaction.CreatedAt
        };
    }
}

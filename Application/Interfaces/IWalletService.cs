using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> AddFundsAsync(int userId, decimal amount, string idempotencyKey);
    Task<WalletResponse> GetWalletByUserIdAsync(int userId);
    Task<IEnumerable<WalletTransactionResponse>> GetTransactionHistoryAsync(int userId, int page = 1, int pageSize = 20);
    Task<WalletResponse> CreditWalletAsync(int userId, decimal amount, string description, string? referenceId = null);
    Task<WalletResponse> DebitWalletAsync(int userId, decimal amount, string description, string? referenceId = null);
}

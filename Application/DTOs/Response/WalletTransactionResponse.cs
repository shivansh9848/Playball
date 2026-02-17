namespace Assignment_Example_HU.Application.DTOs.Response;

public class WalletTransactionResponse
{
    public int TransactionId { get; set; }
    public string Type { get; set; } = string.Empty; // Credit/Debit
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public int? BookingId { get; set; }
    public DateTime CreatedAt { get; set; }
}

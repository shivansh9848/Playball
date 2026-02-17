using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class Transaction
{
    [Key]
    public int TransactionId { get; set; }

    [Required]
    public int WalletId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceId { get; set; } // For idempotency

    public int? BookingId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(WalletId))]
    public virtual Wallet Wallet { get; set; } = null!;

    [ForeignKey(nameof(BookingId))]
    public virtual Booking? Booking { get; set; }
}

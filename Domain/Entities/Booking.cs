using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int CourtId { get; set; }

    [Required]
    public DateTime SlotStartTime { get; set; }

    [Required]
    public DateTime SlotEndTime { get; set; }

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceLocked { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; } = 0;

    public DateTime? LockExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public int? GameId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(CourtId))]
    public virtual Court Court { get; set; } = null!;

    [ForeignKey(nameof(GameId))]
    public virtual Game? Game { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_Example_HU.Domain.Entities;

public class Wallet
{
    [Key]
    public int WalletId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

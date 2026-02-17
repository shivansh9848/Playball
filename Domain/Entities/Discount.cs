using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_Example_HU.Domain.Entities;

public class Discount
{
    [Key]
    public int DiscountId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Scope { get; set; } = string.Empty; // "Venue" or "Court"

    public int? VenueId { get; set; }

    public int? CourtId { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PercentOff { get; set; } // e.g., 20 for 20% off

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(VenueId))]
    public virtual Venue? Venue { get; set; }

    [ForeignKey(nameof(CourtId))]
    public virtual Court? Court { get; set; }
}

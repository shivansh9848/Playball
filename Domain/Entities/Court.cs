using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class Court
{
    [Key]
    public int CourtId { get; set; }

    [Required]
    public int VenueId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SportType SportType { get; set; }

    [Required]
    public int SlotDurationMinutes { get; set; } = 60;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Required]
    [MaxLength(10)]
    public string OpenTime { get; set; } = "06:00"; // HH:mm format

    [Required]
    [MaxLength(10)]
    public string CloseTime { get; set; } = "23:00"; // HH:mm format

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(VenueId))]
    public virtual Venue Venue { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}

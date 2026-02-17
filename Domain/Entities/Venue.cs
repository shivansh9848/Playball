using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class Venue
{
    [Key]
    public int VenueId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string SportsSupported { get; set; } = string.Empty; // Comma-separated SportType values

    [Required]
    public int OwnerId { get; set; }

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedBy { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OwnerId))]
    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Court> Courts { get; set; } = new List<Court>();
    public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}

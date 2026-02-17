using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_Example_HU.Domain.Entities;

public class Rating
{
    [Key]
    public int RatingId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Required]
    [MaxLength(20)]
    public string TargetType { get; set; } = string.Empty; // "Venue", "Court", or "Player"

    public int? VenueId { get; set; }

    public int? CourtId { get; set; }

    public int? TargetUserId { get; set; } // For player ratings

    [Required]
    [Range(1, 5)]
    public int Score { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey(nameof(VenueId))]
    public virtual Venue? Venue { get; set; }

    [ForeignKey(nameof(CourtId))]
    public virtual Court? Court { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public virtual User? TargetUser { get; set; }
}

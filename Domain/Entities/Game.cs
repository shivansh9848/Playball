using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class Game
{
    [Key]
    public int GameId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int VenueId { get; set; }

    [Required]
    public int CourtId { get; set; }

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public int MinPlayers { get; set; }

    [Required]
    public int MaxPlayers { get; set; }

    [Required]
    public int CurrentPlayers { get; set; } = 0;

    [Required]
    public GameStatus Status { get; set; } = GameStatus.Open;

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CreatedBy))]
    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<GameParticipant> Participants { get; set; } = new List<GameParticipant>();
    public virtual ICollection<Waitlist> WaitlistEntries { get; set; } = new List<Waitlist>();

    public int? BookingId { get; set; }
    
    [ForeignKey(nameof(BookingId))]
    public virtual Booking? Booking { get; set; }
}

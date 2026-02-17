using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_Example_HU.Domain.Entities;

public class Waitlist
{
    [Key]
    public int WaitlistId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public int Position { get; set; }

    public bool IsInvited { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

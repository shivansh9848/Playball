using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class GameParticipant
{
    [Key]
    public int GameParticipantId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [Required]
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Accepted;

    // Navigation properties
    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

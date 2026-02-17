using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateGameRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int VenueId { get; set; }

    [Required]
    public int CourtId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [Range(2, 100)]
    public int MinPlayers { get; set; }

    [Required]
    [Range(2, 100)]
    public int MaxPlayers { get; set; }

    public bool IsPublic { get; set; } = true;
}

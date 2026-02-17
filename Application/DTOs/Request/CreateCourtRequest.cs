using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateCourtRequest
{
    [Required]
    public int VenueId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 8)]
    public int SportType { get; set; }

    [Required]
    [Range(30, 240)]
    public int SlotDurationMinutes { get; set; } = 60;

    [Required]
    [Range(0.01, 100000)]
    public decimal BasePrice { get; set; }

    [Required]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string OpenTime { get; set; } = "06:00";

    [Required]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string CloseTime { get; set; } = "23:00";
}

using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateDiscountRequest
{
    [Required]
    [RegularExpression("^(Venue|Court)$")]
    public string Scope { get; set; } = string.Empty;

    public int? VenueId { get; set; }

    public int? CourtId { get; set; }

    [Required]
    [Range(1, 100)]
    public decimal PercentOff { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }
}

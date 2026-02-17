using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class UpdateCourtRequest
{
    [StringLength(200, MinimumLength = 2)]
    public string? Name { get; set; }

    [Range(30, 240)]
    public int? SlotDurationMinutes { get; set; }

    [Range(0.01, 100000)]
    public decimal? BasePrice { get; set; }

    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string? OpenTime { get; set; }

    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string? CloseTime { get; set; }

    public bool? IsActive { get; set; }
}

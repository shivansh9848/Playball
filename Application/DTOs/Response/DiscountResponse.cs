namespace Assignment_Example_HU.Application.DTOs.Response;

public class DiscountResponse
{
    public int DiscountId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? VenueId { get; set; }
    public string? VenueName { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; }
    public decimal PercentOff { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

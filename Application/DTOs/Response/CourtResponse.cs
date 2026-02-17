namespace Assignment_Example_HU.Application.DTOs.Response;

public class CourtResponse
{
    public int CourtId { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

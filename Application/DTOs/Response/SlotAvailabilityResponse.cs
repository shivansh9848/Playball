namespace Assignment_Example_HU.Application.DTOs.Response;

public class SlotAvailabilityResponse
{
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public DateTime SlotStartTime { get; set; }
    public DateTime SlotEndTime { get; set; }
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsAvailable { get; set; }
    public PricingBreakdown PricingBreakdown { get; set; } = new();
}

public class PricingBreakdown
{
    public decimal BasePrice { get; set; }
    public decimal DemandMultiplier { get; set; }
    public decimal TimeMultiplier { get; set; }
    public decimal HistoricalMultiplier { get; set; }
    public decimal DiscountFactor { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
}

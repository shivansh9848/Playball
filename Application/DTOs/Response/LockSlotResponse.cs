namespace Assignment_Example_HU.Application.DTOs.Response;

public class LockSlotResponse
{
    public int CourtId { get; set; }
    public DateTime SlotStartTime { get; set; }
    public DateTime SlotEndTime { get; set; }
    public decimal LockedPrice { get; set; }
    public DateTime LockExpiresAt { get; set; }
    public PricingBreakdown? PricingBreakdown { get; set; }
}

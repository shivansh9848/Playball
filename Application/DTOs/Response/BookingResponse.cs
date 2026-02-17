namespace Assignment_Example_HU.Application.DTOs.Response;

public class BookingResponse
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public DateTime SlotStartTime { get; set; }
    public DateTime SlotEndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PriceLocked { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? LockExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

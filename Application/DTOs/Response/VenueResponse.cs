using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Application.DTOs.Response;

public class VenueResponse
{
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string SportsSupported { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<CourtResponse> Courts { get; set; } = new();
}

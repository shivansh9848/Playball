using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class ApproveVenueRequest
{
    [Required]
    public int ApprovalStatus { get; set; } // 2 = Approved, 3 = Rejected

    public string? RejectionReason { get; set; }
}

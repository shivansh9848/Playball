using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class LockSlotRequest
{
    [Required]
    public int CourtId { get; set; }

    [Required]
    public DateTime SlotStartTime { get; set; }

    [Required]
    public DateTime SlotEndTime { get; set; }
}

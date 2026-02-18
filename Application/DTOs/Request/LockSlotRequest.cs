using System.ComponentModel.DataAnnotations;
using Assignment_Example_HU.Common.Helpers;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class LockSlotRequest : IValidatableObject
{
    [Required(ErrorMessage = "CourtId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "CourtId must be a positive integer.")]
    public int CourtId { get; set; }

    [Required(ErrorMessage = "SlotStartTime is required.")]
    public DateTime SlotStartTime { get; set; }

    [Required(ErrorMessage = "SlotEndTime is required.")]
    public DateTime SlotEndTime { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SlotStartTime <= IstClock.Now)
            yield return new ValidationResult("SlotStartTime must be in the future.", [nameof(SlotStartTime)]);

        if (SlotEndTime <= SlotStartTime)
            yield return new ValidationResult("SlotEndTime must be after SlotStartTime.", [nameof(SlotEndTime)]);
    }
}

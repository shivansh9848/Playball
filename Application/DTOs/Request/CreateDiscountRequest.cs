using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateDiscountRequest : IValidatableObject
{
    [Required(ErrorMessage = "Scope is required. Use 'Venue' or 'Court'.")]
    [RegularExpression("^(Venue|Court)$", ErrorMessage = "Scope must be either 'Venue' or 'Court'.")]
    public string Scope { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "VenueId must be a positive integer.")]
    public int? VenueId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CourtId must be a positive integer.")]
    public int? CourtId { get; set; }

    [Required(ErrorMessage = "PercentOff is required.")]
    [Range(1, 99, ErrorMessage = "PercentOff must be between 1 and 99.")]
    public decimal PercentOff { get; set; }

    [Required(ErrorMessage = "ValidFrom date is required.")]
    public DateTime ValidFrom { get; set; }

    [Required(ErrorMessage = "ValidTo date is required.")]
    public DateTime ValidTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ValidTo <= ValidFrom)
            yield return new ValidationResult("ValidTo must be after ValidFrom.", [nameof(ValidTo)]);

        if (Scope == "Venue" && !VenueId.HasValue)
            yield return new ValidationResult("VenueId is required when Scope is 'Venue'.", [nameof(VenueId)]);

        if (Scope == "Court" && !CourtId.HasValue)
            yield return new ValidationResult("CourtId is required when Scope is 'Court'.", [nameof(CourtId)]);
    }
}

using System.ComponentModel.DataAnnotations;
using Assignment_Example_HU.Common.Helpers;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateGameRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "VenueId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "VenueId must be a positive integer.")]
    public int VenueId { get; set; }

    [Required(ErrorMessage = "CourtId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "CourtId must be a positive integer.")]
    public int CourtId { get; set; }

    [Required(ErrorMessage = "StartTime is required.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "EndTime is required.")]
    public DateTime EndTime { get; set; }

    [Required(ErrorMessage = "MinPlayers is required.")]
    [Range(2, 100, ErrorMessage = "MinPlayers must be between 2 and 100.")]
    public int MinPlayers { get; set; }

    [Required(ErrorMessage = "MaxPlayers is required.")]
    [Range(2, 100, ErrorMessage = "MaxPlayers must be between 2 and 100.")]
    public int MaxPlayers { get; set; }

    public bool IsPublic { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime <= IstClock.Now)
            yield return new ValidationResult("StartTime must be in the future.", [nameof(StartTime)]);

        if (EndTime <= StartTime)
            yield return new ValidationResult("EndTime must be after StartTime.", [nameof(EndTime)]);

        if (MinPlayers > MaxPlayers)
            yield return new ValidationResult("MinPlayers cannot be greater than MaxPlayers.", [nameof(MinPlayers)]);
    }
}

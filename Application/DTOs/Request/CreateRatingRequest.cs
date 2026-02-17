using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateRatingRequest
{
    [Required]
    [Range(1, 5, ErrorMessage = "Score must be between 1 and 5")]
    public int Score { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment must not exceed 1000 characters")]
    public string? Comment { get; set; }

    [Required]
    public int GameId { get; set; }
}

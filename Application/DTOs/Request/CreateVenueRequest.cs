using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class CreateVenueRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public List<int> SportsSupported { get; set; } = new();
}

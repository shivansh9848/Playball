using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class InviteUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

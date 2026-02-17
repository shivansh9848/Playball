using System.ComponentModel.DataAnnotations;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class AssignRoleRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public UserRole NewRole { get; set; }
}

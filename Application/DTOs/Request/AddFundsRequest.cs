using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class AddFundsRequest
{
    [Required]
    [Range(1, 1000000, ErrorMessage = "Amount must be between 1 and 1,000,000")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Idempotency key must not exceed 100 characters")]
    public string IdempotencyKey { get; set; } = string.Empty;
}

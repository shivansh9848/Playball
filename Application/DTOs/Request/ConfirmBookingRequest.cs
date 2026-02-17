using System.ComponentModel.DataAnnotations;

namespace Assignment_Example_HU.Application.DTOs.Request;

public class ConfirmBookingRequest
{
    [Required]
    public int BookingId { get; set; }
}

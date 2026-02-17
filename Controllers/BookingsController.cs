using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("lock-slot")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> LockSlot([FromBody] LockSlotRequest request)
    {
        var response = await _bookingService.LockSlotAsync(User.GetUserId(), request);
        return Ok(response);
    }

    [HttpPost("confirm")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest request)
    {
        var response = await _bookingService.ConfirmBookingAsync(User.GetUserId(), request);
        return Ok(response);
    }

    [HttpPost("cancel/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] string? reason)
    {
        await _bookingService.CancelBookingAsync(User.GetUserId(), bookingId, reason);
        return NoContent();
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync(User.GetUserId());
        return Ok(bookings);
    }

    [HttpGet("{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        return Ok(booking);
    }
}

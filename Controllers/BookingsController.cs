using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
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
        var booking = await _bookingService.LockSlotAsync(User.GetUserId(), request);
        return Ok(ApiResponse<BookingResponse>.SuccessResponse(booking,
            $"Slot locked successfully. Price locked at ₹{booking.PriceLocked:F2}. You have 5 minutes to confirm. Booking ID: {booking.BookingId}."));
    }

    [HttpPost("confirm")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest request)
    {
        var booking = await _bookingService.ConfirmBookingAsync(User.GetUserId(), request);
        return Ok(ApiResponse<BookingResponse>.SuccessResponse(booking,
            $"Booking confirmed successfully! ₹{booking.PriceLocked:F2} has been debited from your wallet. Booking ID: {booking.BookingId}."));
    }

    [HttpPost("cancel/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] string? reason)
    {
        var booking = await _bookingService.CancelBookingAsync(User.GetUserId(), bookingId, reason);
        return Ok(ApiResponse<BookingResponse>.SuccessResponse(booking,
            $"Booking #{bookingId} cancelled successfully. Refund (if applicable) has been processed to your wallet."));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync(User.GetUserId());
        var list = bookings.ToList();
        return Ok(ApiResponse<IEnumerable<BookingResponse>>.SuccessResponse(list,
            $"You have {list.Count} booking(s)."));
    }

    [HttpGet("{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Booking with ID {bookingId} not found."));
        return Ok(ApiResponse<BookingResponse>.SuccessResponse(booking, "Booking details retrieved successfully."));
    }
}

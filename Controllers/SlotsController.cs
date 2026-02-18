using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;

    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    /// <summary>
    /// Get available slots for a court on a given date.
    /// Date format: yyyy-MM-dd (e.g. 2026-02-20)
    /// </summary>
    [HttpGet("available/{courtId}/{date}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SlotAvailabilityResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSlots(int courtId, string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid date format. Use yyyy-MM-dd (e.g. 2026-02-20)"));
        }

        var slots = await _slotService.GetAvailableSlotsAsync(courtId, parsedDate);
        return Ok(ApiResponse<IEnumerable<SlotAvailabilityResponse>>.SuccessResponse(
            slots, $"Available slots for court {courtId} on {date}"));
    }

    /// <summary>
    /// Get slot details with pricing breakdown.
    /// DateTime format: yyyy-MM-ddTHH:mm:ss (e.g. 2026-02-20T09:00:00)
    /// </summary>
    [HttpGet("details/{courtId}/{slotStart}/{slotEnd}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SlotAvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlotDetails(int courtId, DateTime slotStart, DateTime slotEnd)
    {
        var slot = await _slotService.GetSlotDetailsAsync(courtId, slotStart, slotEnd);
        return Ok(ApiResponse<SlotAvailabilityResponse>.SuccessResponse(
            slot!, $"Slot details for court {courtId}"));
    }
}

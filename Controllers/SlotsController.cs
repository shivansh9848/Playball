using Assignment_Example_HU.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("available/{courtId}/{date}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int courtId, DateTime date)
    {
        var slots = await _slotService.GetAvailableSlotsAsync(courtId, date);
        return Ok(slots);
    }

    [HttpGet("details/{courtId}/{slotStart}/{slotEnd}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlotDetails(int courtId, DateTime slotStart, DateTime slotEnd)
    {
        var slot = await _slotService.GetSlotDetailsAsync(courtId, slotStart, slotEnd);
        return Ok(slot);
    }
}

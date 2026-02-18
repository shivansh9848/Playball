using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtsController : ControllerBase
{
    private readonly ICourtService _courtService;

    public CourtsController(ICourtService courtService)
    {
        _courtService = courtService;
    }

    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> CreateCourt([FromBody] CreateCourtRequest request)
    {
        var court = await _courtService.CreateCourtAsync(User.GetUserId(), request);
        return Ok(ApiResponse<CourtResponse>.SuccessResponse(court,
            $"Court '{court.Name}' created successfully under venue ID {court.VenueId}."));
    }

    [HttpPut("{courtId}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> UpdateCourt(int courtId, [FromBody] UpdateCourtRequest request)
    {
        var court = await _courtService.UpdateCourtAsync(User.GetUserId(), courtId, request);
        return Ok(ApiResponse<CourtResponse>.SuccessResponse(court,
            $"Court '{court.Name}' updated successfully."));
    }

    [HttpDelete("{courtId}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> DeleteCourt(int courtId)
    {
        await _courtService.DeleteCourtAsync(User.GetUserId(), courtId);
        return Ok(ApiResponse<object?>.SuccessResponse(null,
            $"Court ID {courtId} has been deleted successfully."));
    }

    [HttpGet("venue/{venueId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourtsByVenue(int venueId)
    {
        var courts = await _courtService.GetCourtsByVenueAsync(venueId);
        var list = courts.ToList();
        return Ok(ApiResponse<IEnumerable<CourtResponse>>.SuccessResponse(list,
            $"{list.Count} court(s) found for venue ID {venueId}."));
    }
}

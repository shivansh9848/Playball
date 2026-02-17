using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateVenue([FromBody] CreateVenueRequest request)
    {
        var venue = await _venueService.CreateVenueAsync(User.GetUserId(), request);
        return Ok(venue);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetMyVenues()
    {
        var venues = await _venueService.GetMyVenuesAsync(User.GetUserId());
        return Ok(venues);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingVenues()
    {
        var venues = await _venueService.GetPendingVenuesAsync();
        return Ok(venues);
    }

    [HttpPost("approve/{venueId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveVenue(int venueId, [FromBody] ApproveVenueRequest request)
    {
        await _venueService.ApproveVenueAsync(User.GetUserId(), venueId, request);
        return NoContent();
    }
}

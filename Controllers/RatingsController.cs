using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    /// <summary>
    /// Rate a venue after game completion
    /// </summary>
    [HttpPost("venue/{venueId}")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> RateVenue(int venueId, [FromBody] CreateRatingRequest request)
    {
        var rating = await _ratingService.RateVenueAsync(User.GetUserId(), venueId, request);
        return Ok(rating);
    }

    /// <summary>
    /// Rate a court after game completion
    /// </summary>
    [HttpPost("court/{courtId}")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> RateCourt(int courtId, [FromBody] CreateRatingRequest request)
    {
        var rating = await _ratingService.RateCourtAsync(User.GetUserId(), courtId, request);
        return Ok(rating);
    }

    /// <summary>
    /// Rate a player after game completion
    /// </summary>
    [HttpPost("player/{playerId}")]
    [Authorize(Roles = "User,GameOwner")]
    public async Task<IActionResult> RatePlayer(int playerId, [FromBody] CreateRatingRequest request)
    {
        var rating = await _ratingService.RatePlayerAsync(User.GetUserId(), playerId, request);
        return Ok(rating);
    }

    /// <summary>
    /// Get all ratings for a venue
    /// </summary>
    [HttpGet("venue/{venueId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenueRatings(int venueId)
    {
        var ratings = await _ratingService.GetVenueRatingsAsync(venueId);
        return Ok(ratings);
    }

    /// <summary>
    /// Get all ratings for a court
    /// </summary>
    [HttpGet("court/{courtId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourtRatings(int courtId)
    {
        var ratings = await _ratingService.GetCourtRatingsAsync(courtId);
        return Ok(ratings);
    }

    /// <summary>
    /// Get all ratings for a player
    /// </summary>
    [HttpGet("player/{playerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayerRatings(int playerId)
    {
        var ratings = await _ratingService.GetPlayerRatingsAsync(playerId);
        return Ok(ratings);
    }
}

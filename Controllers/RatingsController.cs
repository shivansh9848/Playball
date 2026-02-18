using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
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
        return Ok(ApiResponse<RatingResponse>.SuccessResponse(rating,
            $"Thank you! Your {request.Score}/5 rating for venue ID {venueId} has been submitted."));
    }

    /// <summary>
    /// Rate a court after game completion
    /// </summary>
    [HttpPost("court/{courtId}")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> RateCourt(int courtId, [FromBody] CreateRatingRequest request)
    {
        var rating = await _ratingService.RateCourtAsync(User.GetUserId(), courtId, request);
        return Ok(ApiResponse<RatingResponse>.SuccessResponse(rating,
            $"Thank you! Your {request.Score}/5 rating for court ID {courtId} has been submitted."));
    }

    /// <summary>
    /// Rate a player after game completion
    /// </summary>
    [HttpPost("player/{playerId}")]
    [Authorize(Roles = "User,GameOwner")]
    public async Task<IActionResult> RatePlayer(int playerId, [FromBody] CreateRatingRequest request)
    {
        var rating = await _ratingService.RatePlayerAsync(User.GetUserId(), playerId, request);
        return Ok(ApiResponse<RatingResponse>.SuccessResponse(rating,
            $"Thank you! Your {request.Score}/5 rating for player ID {playerId} has been submitted."));
    }

    /// <summary>
    /// Get all ratings for a venue
    /// </summary>
    [HttpGet("venue/{venueId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenueRatings(int venueId)
    {
        var ratings = await _ratingService.GetVenueRatingsAsync(venueId);
        var list = ratings.ToList();
        return Ok(ApiResponse<IEnumerable<RatingResponse>>.SuccessResponse(list,
            $"{list.Count} rating(s) found for venue ID {venueId}."));
    }

    /// <summary>
    /// Get all ratings for a court
    /// </summary>
    [HttpGet("court/{courtId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourtRatings(int courtId)
    {
        var ratings = await _ratingService.GetCourtRatingsAsync(courtId);
        var list = ratings.ToList();
        return Ok(ApiResponse<IEnumerable<RatingResponse>>.SuccessResponse(list,
            $"{list.Count} rating(s) found for court ID {courtId}."));
    }

    /// <summary>
    /// Get all ratings for a player
    /// </summary>
    [HttpGet("player/{playerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayerRatings(int playerId)
    {
        var ratings = await _ratingService.GetPlayerRatingsAsync(playerId);
        var list = ratings.ToList();
        return Ok(ApiResponse<IEnumerable<RatingResponse>>.SuccessResponse(list,
            $"{list.Count} rating(s) found for player ID {playerId}."));
    }
}

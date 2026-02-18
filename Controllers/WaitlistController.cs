using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/games/{gameId}/[controller]")]
public class WaitlistController : ControllerBase
{
    private readonly IWaitlistService _waitlistService;

    public WaitlistController(IWaitlistService waitlistService)
    {
        _waitlistService = waitlistService;
    }

    /// <summary>
    /// Join waitlist for a game
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> JoinWaitlist(int gameId)
    {
        var entry = await _waitlistService.JoinWaitlistAsync(User.GetUserId(), gameId);
        return Ok(ApiResponse<WaitlistResponse>.SuccessResponse(entry,
            $"You have joined the waitlist at position #{entry.Position}. You will be notified if a spot opens up."));
    }

    /// <summary>
    /// Get waitlist for a game (sorted by rating)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetWaitlist(int gameId)
    {
        var waitlist = await _waitlistService.GetWaitlistAsync(gameId);
        var list = waitlist.ToList();
        return Ok(ApiResponse<IEnumerable<WaitlistResponse>>.SuccessResponse(list,
            $"{list.Count} player(s) on the waitlist, sorted by rating."));
    }

    /// <summary>
    /// Invite user from waitlist (game owner only)
    /// </summary>
    [HttpPost("invite/{userId}")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> InviteFromWaitlist(int gameId, int userId)
    {
        await _waitlistService.InviteFromWaitlistAsync(User.GetUserId(), gameId, userId);
        return Ok(ApiResponse<object?>.SuccessResponse(null, $"Player (ID: {userId}) has been invited from the waitlist and added to the game."));
    }

    /// <summary>
    /// Leave waitlist
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> LeaveWaitlist(int gameId)
    {
        await _waitlistService.RemoveFromWaitlistAsync(User.GetUserId(), gameId);
        return Ok(ApiResponse<object?>.SuccessResponse(null, "You have been removed from the waitlist."));
    }
}

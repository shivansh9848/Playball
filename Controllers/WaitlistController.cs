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
    [Authorize(Roles = "User,GameOwner")]
    public async Task<IActionResult> JoinWaitlist(int gameId)
    {
        var entry = await _waitlistService.JoinWaitlistAsync(User.GetUserId(), gameId);
        return Ok(entry);
    }

    /// <summary>
    /// Get waitlist for a game (sorted by rating)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetWaitlist(int gameId)
    {
        var waitlist = await _waitlistService.GetWaitlistAsync(gameId);
        return Ok(waitlist);
    }

    /// <summary>
    /// Invite user from waitlist (game owner only)
    /// </summary>
    [HttpPost("invite/{userId}")]
    [Authorize(Roles = "User,GameOwner")]
    public async Task<IActionResult> InviteFromWaitlist(int gameId, int userId)
    {
        await _waitlistService.InviteFromWaitlistAsync(User.GetUserId(), gameId, userId);
        return NoContent();
    }

    /// <summary>
    /// Leave waitlist
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> LeaveWaitlist(int gameId)
    {
        await _waitlistService.RemoveFromWaitlistAsync(User.GetUserId(), gameId);
        return NoContent();
    }
}

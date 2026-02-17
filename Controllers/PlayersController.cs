using Assignment_Example_HU.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerProfileService _playerProfileService;

    public PlayersController(IPlayerProfileService playerProfileService)
    {
        _playerProfileService = playerProfileService;
    }

    /// <summary>
    /// Get player profile with ratings, games played, and recent reviews
    /// </summary>
    [HttpGet("{userId}/profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayerProfile(int userId)
    {
        var profile = await _playerProfileService.GetPlayerProfileAsync(userId);
        return Ok(profile);
    }
}

using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [Authorize(Roles = "Player,Owner")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = await _gameService.CreateGameAsync(User.GetUserId(), request);
        return Ok(game);
    }

    [HttpPost("{gameId}/join")]
    [Authorize(Roles = "Player,Owner")]
    public async Task<IActionResult> JoinGame(int gameId)
    {
        await _gameService.JoinGameAsync(User.GetUserId(), gameId);
        return NoContent();
    }

    [HttpPost("{gameId}/leave")]
    [Authorize(Roles = "Player,Owner")]
    public async Task<IActionResult> LeaveGame(int gameId)
    {
        await _gameService.LeaveGameAsync(User.GetUserId(), gameId);
        return NoContent();
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicGames()
    {
        var games = await _gameService.GetPublicGamesAsync();
        return Ok(games);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyGames()
    {
        var games = await _gameService.GetMyGamesAsync(User.GetUserId());
        return Ok(games);
    }

    [HttpGet("{gameId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameById(int gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);
        return Ok(game);
    }
}

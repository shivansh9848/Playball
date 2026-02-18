using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
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
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = await _gameService.CreateGameAsync(User.GetUserId(), request);
        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, $"Game '{game.Title}' created successfully. You have been assigned as Game Owner."));
    }

    [HttpPost("{gameId}/join")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> JoinGame(int gameId)
    {
        var game = await _gameService.JoinGameAsync(User.GetUserId(), gameId);
        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, $"You have successfully joined '{game.Title}'. Current players: {game.CurrentPlayers}/{game.MaxPlayers}."));
    }

    [HttpPost("{gameId}/leave")]
    [Authorize(Roles = "User,GameOwner,VenueOwner")]
    public async Task<IActionResult> LeaveGame(int gameId)
    {
        var game = await _gameService.LeaveGameAsync(User.GetUserId(), gameId);
        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, $"You have left '{game.Title}'."));
    }

    [HttpPost("{gameId}/approve/{participantId}")]
    [Authorize(Roles = "User,GameOwner")]
    public async Task<IActionResult> ApproveParticipant(int gameId, int participantId)
    {
        var game = await _gameService.ApproveParticipantAsync(User.GetUserId(), gameId, participantId);
        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, "Participant approved successfully."));
    }

    [HttpPost("{gameId}/complete")]
    [Authorize(Roles = "User,GameOwner,VenueOwner,Admin")]
    public async Task<IActionResult> CompleteGame(int gameId)
    {
        await _gameService.CompleteGameAsync(gameId);
        // Fetch updated game to return
        var game = await _gameService.GetGameByIdAsync(gameId);
        if (game == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Game with ID {gameId} not found."));

        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, "Game completed and payouts processed."));
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicGames()
    {
        var games = await _gameService.GetPublicGamesAsync();
        var list = games.ToList();
        return Ok(ApiResponse<IEnumerable<GameResponse>>.SuccessResponse(list, $"{list.Count} public game(s) found."));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyGames()
    {
        var games = await _gameService.GetMyGamesAsync(User.GetUserId());
        var list = games.ToList();
        return Ok(ApiResponse<IEnumerable<GameResponse>>.SuccessResponse(list, $"You have {list.Count} game(s)."));
    }

    [HttpGet("{gameId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameById(int gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);
        if (game == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Game with ID {gameId} not found."));
        return Ok(ApiResponse<GameResponse>.SuccessResponse(game, "Game details retrieved successfully."));
    }
}

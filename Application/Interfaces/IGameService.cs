using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IGameService
{
    Task<GameResponse> CreateGameAsync(int userId, CreateGameRequest request);
    Task<GameResponse> JoinGameAsync(int userId, int gameId);
    Task<GameResponse> LeaveGameAsync(int userId, int gameId);
    Task<IEnumerable<GameResponse>> GetPublicGamesAsync();
    Task<IEnumerable<GameResponse>> GetMyGamesAsync(int userId);
    Task<GameResponse?> GetGameByIdAsync(int gameId);
    Task AutoCancelGamesAsync();
}

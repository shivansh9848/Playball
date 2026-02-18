using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;
using Assignment_Example_HU.Common.Helpers;

namespace Playball.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IRepository<GameParticipant>> _participantRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _participantRepoMock = new Mock<IRepository<GameParticipant>>();
        _userRepoMock = new Mock<IUserRepository>();
        _gameService = new GameService(_gameRepoMock.Object, _participantRepoMock.Object, _userRepoMock.Object);
    }

    private Game CreateGame(int id = 1, int createdBy = 1, GameStatus status = GameStatus.Open, int current = 2, int max = 10, int min = 2) =>
        new()
        {
            GameId = id, Title = "Test Game", VenueId = 1, CourtId = 1, CreatedBy = createdBy,
            StartTime = IstClock.Now.AddHours(2), EndTime = IstClock.Now.AddHours(4),
            MinPlayers = min, MaxPlayers = max, CurrentPlayers = current, Status = status,
            IsPublic = true, Creator = new User { UserId = createdBy, FullName = "Creator" },
            Participants = new List<GameParticipant>
            {
                new() { UserId = createdBy, IsActive = true, User = new User { UserId = createdBy, FullName = "Creator" } }
            }
        };

    [Fact]
    public async Task CreateGameAsync_ShouldCreateGame_WhenValidRequest()
    {
        var request = new CreateGameRequest
        {
            Title = "Football Match", VenueId = 1, CourtId = 1,
            StartTime = IstClock.Now.AddHours(2), EndTime = IstClock.Now.AddHours(4),
            MinPlayers = 4, MaxPlayers = 10, IsPublic = true
        };
        var game = CreateGame();
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(It.IsAny<int>())).ReturnsAsync(game);

        var result = await _gameService.CreateGameAsync(1, request);

        Assert.Equal("Test Game", result.Title);
        _gameRepoMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        _participantRepoMock.Verify(r => r.AddAsync(It.IsAny<GameParticipant>()), Times.Once);
    }

    [Fact]
    public async Task CreateGameAsync_ShouldThrow_WhenMinGreaterThanMax()
    {
        var request = new CreateGameRequest
        {
            Title = "Bad Game", VenueId = 1, CourtId = 1,
            StartTime = IstClock.Now.AddHours(2), EndTime = IstClock.Now.AddHours(4),
            MinPlayers = 15, MaxPlayers = 5
        };

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.CreateGameAsync(1, request));
    }

    [Fact]
    public async Task CreateGameAsync_ShouldThrow_WhenStartAfterEnd()
    {
        var request = new CreateGameRequest
        {
            Title = "Bad Game", VenueId = 1, CourtId = 1,
            StartTime = IstClock.Now.AddHours(4), EndTime = IstClock.Now.AddHours(2),
            MinPlayers = 2, MaxPlayers = 10
        };

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.CreateGameAsync(1, request));
    }

    [Fact]
    public async Task CreateGameAsync_ShouldThrow_WhenStartInPast()
    {
        var request = new CreateGameRequest
        {
            Title = "Past Game", VenueId = 1, CourtId = 1,
            StartTime = IstClock.Now.AddHours(-1), EndTime = IstClock.Now.AddHours(1),
            MinPlayers = 2, MaxPlayers = 10
        };

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.CreateGameAsync(1, request));
    }

    [Fact]
    public async Task JoinGameAsync_ShouldAddParticipant_WhenGameOpen()
    {
        var game = CreateGame();
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        var result = await _gameService.JoinGameAsync(99, 1);

        _participantRepoMock.Verify(r => r.AddAsync(It.IsAny<GameParticipant>()), Times.Once);
    }

    [Fact]
    public async Task JoinGameAsync_ShouldThrow_WhenGameNotFound()
    {
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(99)).ReturnsAsync((Game?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _gameService.JoinGameAsync(1, 99));
    }

    [Fact]
    public async Task JoinGameAsync_ShouldThrow_WhenGameNotOpen()
    {
        var game = CreateGame(status: GameStatus.Completed);
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.JoinGameAsync(99, 1));
    }

    [Fact]
    public async Task JoinGameAsync_ShouldThrow_WhenAlreadyInGame()
    {
        var game = CreateGame();
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.JoinGameAsync(1, 1));
    }

    [Fact]
    public async Task JoinGameAsync_ShouldThrow_WhenGameFull()
    {
        var game = CreateGame(current: 10, max: 10);
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.JoinGameAsync(99, 1));
    }

    [Fact]
    public async Task LeaveGameAsync_ShouldRemoveParticipant()
    {
        var game = CreateGame();
        game.Participants.Add(new GameParticipant { UserId = 99, IsActive = true, User = new User { UserId = 99, FullName = "Player" } });
        game.CurrentPlayers = 3;
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        var result = await _gameService.LeaveGameAsync(99, 1);

        _participantRepoMock.Verify(r => r.UpdateAsync(It.IsAny<GameParticipant>()), Times.Once);
    }

    [Fact]
    public async Task LeaveGameAsync_ShouldThrow_WhenCreatorLeavesGame()
    {
        var game = CreateGame();
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.LeaveGameAsync(1, 1));
    }

    [Fact]
    public async Task LeaveGameAsync_ShouldThrow_WhenNotInGame()
    {
        var game = CreateGame();
        _gameRepoMock.Setup(r => r.GetGameWithParticipantsAsync(1)).ReturnsAsync(game);

        await Assert.ThrowsAsync<BusinessException>(() => _gameService.LeaveGameAsync(55, 1));
    }

    [Fact]
    public async Task AutoCancelGamesAsync_ShouldCancelGames_BelowMinPlayers()
    {
        var games = new List<Game>
        {
            CreateGame(id: 1, current: 1, min: 4),
            CreateGame(id: 2, current: 2, min: 5)
        };
        _gameRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Game, bool>>>())).ReturnsAsync(games);

        await _gameService.AutoCancelGamesAsync();

        _gameRepoMock.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.Status == GameStatus.Cancelled)), Times.Exactly(2));
    }

    [Fact]
    public async Task GetGameByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _gameRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Game?)null);

        var result = await _gameService.GetGameByIdAsync(99);

        Assert.Null(result);
    }
}

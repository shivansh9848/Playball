using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class WaitlistServiceTests
{
    private readonly Mock<IRepository<Waitlist>> _waitlistRepoMock;
    private readonly Mock<IRepository<Game>> _gameRepoMock;
    private readonly Mock<IRepository<GameParticipant>> _gpRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRatingService> _ratingServiceMock;
    private readonly WaitlistService _waitlistService;

    public WaitlistServiceTests()
    {
        _waitlistRepoMock = new Mock<IRepository<Waitlist>>();
        _gameRepoMock = new Mock<IRepository<Game>>();
        _gpRepoMock = new Mock<IRepository<GameParticipant>>();
        _userRepoMock = new Mock<IUserRepository>();
        _ratingServiceMock = new Mock<IRatingService>();

        _waitlistService = new WaitlistService(
            _waitlistRepoMock.Object, _gameRepoMock.Object, _gpRepoMock.Object,
            _userRepoMock.Object, _ratingServiceMock.Object);
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldAddToWaitlist_WhenGameIsFull()
    {
        // Arrange
        var game = new Game { GameId = 1, MaxPlayers = 2 };
        var participants = new List<GameParticipant>
        {
            new() { UserId = 10, GameId = 1 },
            new() { UserId = 11, GameId = 1 }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participants);
        _waitlistRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Waitlist, bool>>>()))
            .ReturnsAsync(new List<Waitlist>());
        _waitlistRepoMock.Setup(r => r.AddAsync(It.IsAny<Waitlist>())).ReturnsAsync((Waitlist w) => w);
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { FullName = "Test" });
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, It.IsAny<int>())).ReturnsAsync(4.0m);

        // Act
        var result = await _waitlistService.JoinWaitlistAsync(5, 1);

        // Assert
        Assert.Equal(1, result.GameId);
        _waitlistRepoMock.Verify(r => r.AddAsync(It.IsAny<Waitlist>()), Times.Once);
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldThrow_WhenGameNotFull()
    {
        // Arrange
        var game = new Game { GameId = 1, MaxPlayers = 10 };
        var participants = new List<GameParticipant>
        {
            new() { UserId = 10, GameId = 1 }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participants);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _waitlistService.JoinWaitlistAsync(5, 1));
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldThrow_WhenAlreadyInGame()
    {
        // Arrange
        var game = new Game { GameId = 1, MaxPlayers = 2 };
        var participants = new List<GameParticipant>
        {
            new() { UserId = 5, GameId = 1 }, // same user
            new() { UserId = 10, GameId = 1 }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participants);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _waitlistService.JoinWaitlistAsync(5, 1));
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldThrow_WhenAlreadyOnWaitlist()
    {
        // Arrange
        var game = new Game { GameId = 1, MaxPlayers = 2 };
        var participants = new List<GameParticipant>
        {
            new() { UserId = 10, GameId = 1 },
            new() { UserId = 11, GameId = 1 }
        };
        var existingEntry = new Waitlist { UserId = 5, GameId = 1 };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participants);

        // First call for "already on waitlist" check returns existing entry
        _waitlistRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<Waitlist, bool>>>()))
            .ReturnsAsync(new List<Waitlist> { existingEntry });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _waitlistService.JoinWaitlistAsync(5, 1));
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldThrow_WhenWaitlistFull()
    {
        // Arrange
        var game = new Game { GameId = 1, MaxPlayers = 2 };
        var participants = new List<GameParticipant>
        {
            new() { UserId = 10, GameId = 1 },
            new() { UserId = 11, GameId = 1 }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participants);

        // First call: no existing entry for user; Second call: 10 entries = full waitlist
        var tenEntries = Enumerable.Range(1, 10).Select(i => new Waitlist { UserId = 100 + i, GameId = 1 }).ToList();
        _waitlistRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<Waitlist, bool>>>()))
            .ReturnsAsync(new List<Waitlist>()) // user not on waitlist
            .ReturnsAsync(tenEntries); // waitlist has 10 entries

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _waitlistService.JoinWaitlistAsync(5, 1));
    }

    [Fact]
    public async Task JoinWaitlistAsync_ShouldThrow_WhenGameNotFound()
    {
        // Arrange
        _gameRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Game?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _waitlistService.JoinWaitlistAsync(1, 999));
    }

    [Fact]
    public async Task InviteFromWaitlistAsync_ShouldMarkAsInvited_WhenGameOwner()
    {
        // Arrange
        var game = new Game { GameId = 1, CreatedBy = 1 };
        var waitlistEntry = new Waitlist { WaitlistId = 1, GameId = 1, UserId = 5, IsInvited = false };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _waitlistRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Waitlist, bool>>>()))
            .ReturnsAsync(new List<Waitlist> { waitlistEntry });

        // Act
        await _waitlistService.InviteFromWaitlistAsync(1, 1, 5);

        // Assert
        Assert.True(waitlistEntry.IsInvited);
        _waitlistRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Waitlist>()), Times.Once);
    }

    [Fact]
    public async Task InviteFromWaitlistAsync_ShouldThrow_WhenNotGameOwner()
    {
        // Arrange
        var game = new Game { GameId = 1, CreatedBy = 1 };
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _waitlistService.InviteFromWaitlistAsync(99, 1, 5)); // userId 99 is not owner
    }

    [Fact]
    public async Task GetWaitlistAsync_ShouldReturnSortedByRating()
    {
        // Arrange
        var entries = new List<Waitlist>
        {
            new() { WaitlistId = 1, GameId = 1, UserId = 10, Position = 1, JoinedAt = DateTime.UtcNow },
            new() { WaitlistId = 2, GameId = 1, UserId = 20, Position = 2, JoinedAt = DateTime.UtcNow },
        };

        _waitlistRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Waitlist, bool>>>()))
            .ReturnsAsync(entries);
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, 10)).ReturnsAsync(3.0m);
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, 20)).ReturnsAsync(5.0m);
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { FullName = "User" });
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Game { GameId = 1, Title = "Game" });

        // Act
        var result = (await _waitlistService.GetWaitlistAsync(1)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        // User 20 (rating 5.0) should be first
        Assert.Equal(20, result[0].UserId);
        Assert.Equal(10, result[1].UserId);
    }
}

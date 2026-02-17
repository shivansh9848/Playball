using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class RatingServiceTests
{
    private readonly Mock<IRepository<Rating>> _ratingRepoMock;
    private readonly Mock<IRepository<Game>> _gameRepoMock;
    private readonly Mock<IRepository<Venue>> _venueRepoMock;
    private readonly Mock<IRepository<Court>> _courtRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly RatingService _ratingService;

    public RatingServiceTests()
    {
        _ratingRepoMock = new Mock<IRepository<Rating>>();
        _gameRepoMock = new Mock<IRepository<Game>>();
        _venueRepoMock = new Mock<IRepository<Venue>>();
        _courtRepoMock = new Mock<IRepository<Court>>();
        _userRepoMock = new Mock<IUserRepository>();

        _ratingService = new RatingService(
            _ratingRepoMock.Object, _gameRepoMock.Object, _venueRepoMock.Object,
            _courtRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task RateVenueAsync_ShouldCreateRating_WhenGameCompleted()
    {
        // Arrange
        var game = new Game { GameId = 1, Status = GameStatus.Completed };
        var venue = new Venue { VenueId = 1, Name = "Test Venue" };
        var user = new User { UserId = 1, FullName = "Reviewer" };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _ratingRepoMock.Setup(r => r.AddAsync(It.IsAny<Rating>())).ReturnsAsync((Rating r) => r);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var request = new CreateRatingRequest { Score = 5, Comment = "Great venue!", GameId = 1 };

        // Act
        var result = await _ratingService.RateVenueAsync(1, 1, request);

        // Assert
        Assert.Equal(5, result.Score);
        Assert.Equal("Venue", result.TargetType);
        _ratingRepoMock.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Once);
    }

    [Fact]
    public async Task RateVenueAsync_ShouldThrow_WhenGameNotCompleted()
    {
        // Arrange
        var game = new Game { GameId = 1, Status = GameStatus.Open };
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        var request = new CreateRatingRequest { Score = 5, GameId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _ratingService.RateVenueAsync(1, 1, request));
    }

    [Fact]
    public async Task RateVenueAsync_ShouldThrow_WhenDuplicateRating()
    {
        // Arrange
        var game = new Game { GameId = 1, Status = GameStatus.Completed };
        var venue = new Venue { VenueId = 1 };
        var existingRating = new Rating { RatingId = 1, UserId = 1, GameId = 1, VenueId = 1 };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating> { existingRating });

        var request = new CreateRatingRequest { Score = 4, GameId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _ratingService.RateVenueAsync(1, 1, request));
    }

    [Fact]
    public async Task RatePlayerAsync_ShouldThrow_WhenRatingSelf()
    {
        // Arrange
        var request = new CreateRatingRequest { Score = 5, GameId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _ratingService.RatePlayerAsync(1, 1, request)); // userId == targetUserId
    }

    [Fact]
    public async Task RatePlayerAsync_ShouldCreateRating_WhenValid()
    {
        // Arrange
        var game = new Game { GameId = 1, Status = GameStatus.Completed };
        var targetUser = new User { UserId = 2, FullName = "Target Player" };
        var reviewer = new User { UserId = 1, FullName = "Reviewer" };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(targetUser);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reviewer);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _ratingRepoMock.Setup(r => r.AddAsync(It.IsAny<Rating>())).ReturnsAsync((Rating r) => r);

        var request = new CreateRatingRequest { Score = 4, Comment = "Good player", GameId = 1 };

        // Act
        var result = await _ratingService.RatePlayerAsync(1, 2, request);

        // Assert
        Assert.Equal(4, result.Score);
        Assert.Equal("Player", result.TargetType);
    }

    [Fact]
    public async Task RateCourtAsync_ShouldThrow_WhenCourtNotFound()
    {
        // Arrange
        var game = new Game { GameId = 1, Status = GameStatus.Completed };
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _courtRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Court?)null);

        var request = new CreateRatingRequest { Score = 5, GameId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _ratingService.RateCourtAsync(1, 99, request));
    }

    [Fact]
    public async Task GetVenueRatingsAsync_ShouldReturnRatings()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new Rating { RatingId = 1, UserId = 1, GameId = 1, TargetType = "Venue", VenueId = 1, Score = 5 },
            new Rating { RatingId = 2, UserId = 2, GameId = 2, TargetType = "Venue", VenueId = 1, Score = 4 }
        };
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(ratings);
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { FullName = "User" });
        _venueRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Venue { VenueId = 1, Name = "Test Venue" });

        // Act
        var result = await _ratingService.GetVenueRatingsAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAverageRatingAsync_ShouldCalculateAverage()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new Rating { Score = 5, TargetType = "Player", TargetUserId = 1 },
            new Rating { Score = 3, TargetType = "Player", TargetUserId = 1 },
            new Rating { Score = 4, TargetType = "Player", TargetUserId = 1 }
        };
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(ratings);

        // Act
        var avg = await _ratingService.GetAverageRatingAsync(playerId: 1);

        // Assert
        Assert.Equal(4m, avg);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnZero_WhenNoRatings()
    {
        // Arrange
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());

        // Act
        var avg = await _ratingService.GetAverageRatingAsync(playerId: 99);

        // Assert
        Assert.Equal(0m, avg);
    }

    [Fact]
    public async Task RateVenueAsync_ShouldThrow_WhenGameNotFound()
    {
        // Arrange
        _gameRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Game?)null);

        var request = new CreateRatingRequest { Score = 5, GameId = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _ratingService.RateVenueAsync(1, 1, request));
    }
}

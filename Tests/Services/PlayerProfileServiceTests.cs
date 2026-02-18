using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;
using Assignment_Example_HU.Common.Helpers;

namespace Playball.Tests.Services;

public class PlayerProfileServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRepository<GameParticipant>> _gpRepoMock;
    private readonly Mock<IRatingService> _ratingServiceMock;
    private readonly PlayerProfileService _profileService;

    public PlayerProfileServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _gpRepoMock = new Mock<IRepository<GameParticipant>>();
        _ratingServiceMock = new Mock<IRatingService>();

        _profileService = new PlayerProfileService(
            _userRepoMock.Object, _gpRepoMock.Object, _ratingServiceMock.Object);
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ShouldReturnProfile_WithAllData()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FullName = "John Doe",
            Email = "john@example.com",
            Role = UserRole.User,
            CreatedAt = IstClock.Now
        };

        var participations = new List<GameParticipant>
        {
            new() { UserId = 1, GameId = 1 },
            new() { UserId = 1, GameId = 2 },
            new() { UserId = 1, GameId = 3 }
        };

        var ratings = new List<RatingResponse>
        {
            new() { RatingId = 1, Score = 5, Comment = "Great!", CreatedAt = IstClock.Now },
            new() { RatingId = 2, Score = 4, Comment = "Good", CreatedAt = IstClock.Now.AddMinutes(-1) }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(participations);
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, 1)).ReturnsAsync(4.5m);
        _ratingServiceMock.Setup(r => r.GetPlayerRatingsAsync(1)).ReturnsAsync(ratings);

        // Act
        var result = await _profileService.GetPlayerProfileAsync(1);

        // Assert
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal(3, result.TotalGamesPlayed);
        Assert.Equal(4.5m, result.AverageRating);
        Assert.Equal(2, result.TotalRatingsReceived);
        Assert.Equal(2, result.RecentReviews.Count);
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _profileService.GetPlayerProfileAsync(999));
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ShouldReturnZeroRating_WhenNoRatings()
    {
        // Arrange
        var user = new User { UserId = 1, FullName = "New User", Email = "new@example.com", Role = UserRole.User };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(new List<GameParticipant>());
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, 1)).ReturnsAsync(0m);
        _ratingServiceMock.Setup(r => r.GetPlayerRatingsAsync(1)).ReturnsAsync(new List<RatingResponse>());

        // Act
        var result = await _profileService.GetPlayerProfileAsync(1);

        // Assert
        Assert.Equal(0m, result.AverageRating);
        Assert.Equal(0, result.TotalGamesPlayed);
        Assert.Empty(result.RecentReviews);
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ShouldLimitRecentReviewsTo10()
    {
        // Arrange
        var user = new User { UserId = 1, FullName = "Pro Player", Email = "pro@example.com", Role = UserRole.User };
        var ratings = Enumerable.Range(1, 15).Select(i => new RatingResponse
        {
            RatingId = i,
            Score = (i % 5) + 1,
            Comment = $"Review {i}",
            CreatedAt = IstClock.Now.AddMinutes(-i)
        }).ToList();

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _gpRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GameParticipant, bool>>>()))
            .ReturnsAsync(new List<GameParticipant>());
        _ratingServiceMock.Setup(r => r.GetAverageRatingAsync(null, null, 1)).ReturnsAsync(3.5m);
        _ratingServiceMock.Setup(r => r.GetPlayerRatingsAsync(1)).ReturnsAsync(ratings);

        // Act
        var result = await _profileService.GetPlayerProfileAsync(1);

        // Assert
        Assert.Equal(10, result.RecentReviews.Count); // Limited to 10
        Assert.Equal(15, result.TotalRatingsReceived);
    }
}

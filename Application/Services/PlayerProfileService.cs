using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class PlayerProfileService : IPlayerProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<GameParticipant> _gameParticipantRepository;
    private readonly IRatingService _ratingService;

    public PlayerProfileService(
        IUserRepository userRepository,
        IRepository<GameParticipant> gameParticipantRepository,
        IRatingService ratingService)
    {
        _userRepository = userRepository;
        _gameParticipantRepository = gameParticipantRepository;
        _ratingService = ratingService;
    }

    public async Task<PlayerProfileResponse> GetPlayerProfileAsync(int userId)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

        // Get games played count
        var gameParticipations = await _gameParticipantRepository.FindAsync(gp => gp.UserId == userId);
        var totalGamesPlayed = gameParticipations.Count();

        // Get average rating
        var averageRating = await _ratingService.GetAverageRatingAsync(playerId: userId);

        // Get recent reviews (last 10)
        var allRatings = await _ratingService.GetPlayerRatingsAsync(userId);
        var recentReviews = allRatings
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToList();

        return new PlayerProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AverageRating = averageRating,
            TotalGamesPlayed = totalGamesPlayed,
            TotalRatingsReceived = allRatings.Count(),
            RecentReviews = recentReviews,
            CreatedAt = user.CreatedAt
        };
    }
}

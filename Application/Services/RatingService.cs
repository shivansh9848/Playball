using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRepository<Rating> _ratingRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IRepository<Venue> _venueRepository;
    private readonly IRepository<Court> _courtRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<GameParticipant> _participantRepository;

    public RatingService(
        IRepository<Rating> ratingRepository,
        IRepository<Game> gameRepository,
        IRepository<Venue> venueRepository,
        IRepository<Court> courtRepository,
        IUserRepository userRepository,
        IRepository<GameParticipant> participantRepository)
    {
        _ratingRepository = ratingRepository;
        _gameRepository = gameRepository;
        _venueRepository = venueRepository;
        _courtRepository = courtRepository;
        _userRepository = userRepository;
        _participantRepository = participantRepository;
    }

    public async Task<RatingResponse> RateVenueAsync(int userId, int venueId, CreateRatingRequest request)
    {
        // Verify game exists and is completed
        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game == null)
            throw new NotFoundException("Game", request.GameId);

        if (game.Status != GameStatus.Completed)
            throw new BusinessException("You can only rate after the game is completed");

        // Verify user was a participant
        var participant = await _participantRepository.FirstOrDefaultAsync(p => p.GameId == request.GameId && p.UserId == userId && p.IsActive);
        if (participant == null)
            throw new BusinessException("You must be a participant of the game to rate it.");

        // Verify venue exists
        var venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null)
            throw new NotFoundException("Venue", venueId);

        // Prevent owner from rating their own venue
        if (venue.OwnerId == userId)
            throw new BusinessException("You cannot rate your own venue.");

        // Check for duplicate rating
        var existingRatings = await _ratingRepository.FindAsync(r =>
            r.UserId == userId &&
            r.GameId == request.GameId &&
            r.TargetType == "Venue" &&
            r.VenueId == venueId);

        if (existingRatings.Any())
            throw new BusinessException("You have already rated this venue for this game");

        // Create rating
        var rating = new Rating
        {
            UserId = userId,
            GameId = request.GameId,
            TargetType = "Venue",
            VenueId = venueId,
            Score = request.Score,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddAsync(rating);
        await _ratingRepository.SaveChangesAsync();

        return await MapToResponseAsync(rating);
    }

    public async Task<RatingResponse> RateCourtAsync(int userId, int courtId, CreateRatingRequest request)
    {
        // Verify game exists and is completed
        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game == null)
            throw new NotFoundException("Game", request.GameId);

        if (game.Status != GameStatus.Completed)
            throw new BusinessException("You can only rate after the game is completed");

        // Verify user was a participant
        var participant = await _participantRepository.FirstOrDefaultAsync(p => p.GameId == request.GameId && p.UserId == userId && p.IsActive);
        if (participant == null)
            throw new BusinessException("You must be a participant of the game to rate it.");

        // Verify court exists
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        // Verify venue exists to check owner
        var venue = await _venueRepository.GetByIdAsync(court.VenueId);
        if (venue != null && venue.OwnerId == userId)
            throw new BusinessException("You cannot rate your own court.");

        // Check for duplicate rating
        var existingRatings = await _ratingRepository.FindAsync(r =>
            r.UserId == userId &&
            r.GameId == request.GameId &&
            r.TargetType == "Court" &&
            r.CourtId == courtId);

        if (existingRatings.Any())
            throw new BusinessException("You have already rated this court for this game");

        // Create rating
        var rating = new Rating
        {
            UserId = userId,
            GameId = request.GameId,
            TargetType = "Court",
            CourtId = courtId,
            Score = request.Score,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddAsync(rating);
        await _ratingRepository.SaveChangesAsync();

        return await MapToResponseAsync(rating);
    }

    public async Task<RatingResponse> RatePlayerAsync(int userId, int targetUserId, CreateRatingRequest request)
    {
        if (userId == targetUserId)
            throw new BusinessException("You cannot rate yourself");

        // Verify game exists and is completed
        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game == null)
            throw new NotFoundException("Game", request.GameId);

        if (game.Status != GameStatus.Completed)
            throw new BusinessException("You can only rate after the game is completed");

        // Verify user was a participant
        var participant = await _participantRepository.FirstOrDefaultAsync(p => p.GameId == request.GameId && p.UserId == userId && p.IsActive);
        if (participant == null)
            throw new BusinessException("You must be a participant of the game to rate players.");

        // Verify target user exists
        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException("User", targetUserId);

        // Verify target user was also a participant
        var targetParticipant = await _participantRepository.FirstOrDefaultAsync(p => p.GameId == request.GameId && p.UserId == targetUserId && p.IsActive);
        if (targetParticipant == null)
            throw new BusinessException("Target player was not a participant in this game.");

        // Check for duplicate rating
        var existingRatings = await _ratingRepository.FindAsync(r =>
            r.UserId == userId &&
            r.GameId == request.GameId &&
            r.TargetType == "Player" &&
            r.TargetUserId == targetUserId);

        if (existingRatings.Any())
            throw new BusinessException("You have already rated this player for this game");

        // Create rating
        var rating = new Rating
        {
            UserId = userId,
            GameId = request.GameId,
            TargetType = "Player",
            TargetUserId = targetUserId,
            Score = request.Score,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddAsync(rating);
        await _ratingRepository.SaveChangesAsync();

        // Update target user's aggregated rating
        var userRatings = await _ratingRepository.FindAsync(r => r.TargetType == "Player" && r.TargetUserId == targetUserId);
        if (userRatings.Any())
        {
            targetUser.AggregatedRating = (decimal)userRatings.Average(r => r.Score);
            var playedGames = await _participantRepository.FindAsync(p => p.UserId == targetUserId && p.IsActive);
            targetUser.GamesPlayed = playedGames.Count();
            await _userRepository.UpdateAsync(targetUser);
            await _userRepository.SaveChangesAsync();
        }

        return await MapToResponseAsync(rating);
    }

    public async Task<IEnumerable<RatingResponse>> GetVenueRatingsAsync(int venueId)
    {
        var ratings = await _ratingRepository.FindAsync(r =>
            r.TargetType == "Venue" &&
            r.VenueId == venueId);

        var result = new List<RatingResponse>();
        foreach (var rating in ratings)
        {
            result.Add(await MapToResponseAsync(rating));
        }
        return result;
    }

    public async Task<IEnumerable<RatingResponse>> GetCourtRatingsAsync(int courtId)
    {
        var ratings = await _ratingRepository.FindAsync(r =>
            r.TargetType == "Court" &&
            r.CourtId == courtId);

        var result = new List<RatingResponse>();
        foreach (var rating in ratings)
        {
            result.Add(await MapToResponseAsync(rating));
        }
        return result;
    }

    public async Task<IEnumerable<RatingResponse>> GetPlayerRatingsAsync(int playerId)
    {
        var ratings = await _ratingRepository.FindAsync(r =>
            r.TargetType == "Player" &&
            r.TargetUserId == playerId);

        var result = new List<RatingResponse>();
        foreach (var rating in ratings)
        {
            result.Add(await MapToResponseAsync(rating));
        }
        return result;
    }

    public async Task<decimal> GetAverageRatingAsync(int? venueId = null, int? courtId = null, int? playerId = null)
    {
        IEnumerable<Rating> ratings;

        if (venueId.HasValue)
        {
            ratings = await _ratingRepository.FindAsync(r =>
                r.TargetType == "Venue" &&
                r.VenueId == venueId.Value);
        }
        else if (courtId.HasValue)
        {
            ratings = await _ratingRepository.FindAsync(r =>
                r.TargetType == "Court" &&
                r.CourtId == courtId.Value);
        }
        else if (playerId.HasValue)
        {
            ratings = await _ratingRepository.FindAsync(r =>
                r.TargetType == "Player" &&
                r.TargetUserId == playerId.Value);
        }
        else
        {
            return 0;
        }

        if (!ratings.Any())
            return 0;

        return (decimal)ratings.Average(r => r.Score);
    }

    private async Task<RatingResponse> MapToResponseAsync(Rating rating)
    {
        var user = await _userRepository.GetByIdAsync(rating.UserId);
        var response = new RatingResponse
        {
            RatingId = rating.RatingId,
            UserId = rating.UserId,
            UserName = user?.FullName ?? "Unknown",
            GameId = rating.GameId,
            TargetType = rating.TargetType,
            Score = rating.Score,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt
        };

        // Load target details based on type
        if (rating.TargetType == "Venue" && rating.VenueId.HasValue)
        {
            var venue = await _venueRepository.GetByIdAsync(rating.VenueId.Value);
            response.VenueId = rating.VenueId;
            response.VenueName = venue?.Name ?? "Unknown";
        }
        else if (rating.TargetType == "Court" && rating.CourtId.HasValue)
        {
            var court = await _courtRepository.GetByIdAsync(rating.CourtId.Value);
            response.CourtId = rating.CourtId;
            response.CourtName = court?.Name ?? "Unknown";
        }
        else if (rating.TargetType == "Player" && rating.TargetUserId.HasValue)
        {
            var targetUser = await _userRepository.GetByIdAsync(rating.TargetUserId.Value);
            response.TargetUserId = rating.TargetUserId;
            response.TargetUserName = targetUser?.FullName ?? "Unknown";
        }

        return response;
    }
}

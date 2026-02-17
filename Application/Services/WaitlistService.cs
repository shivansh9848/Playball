using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class WaitlistService : IWaitlistService
{
    private readonly IRepository<Waitlist> _waitlistRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IRepository<GameParticipant> _gameParticipantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRatingService _ratingService;
    private const int MaxWaitlistSize = 10;

    public WaitlistService(
        IRepository<Waitlist> waitlistRepository,
        IRepository<Game> gameRepository,
        IRepository<GameParticipant> gameParticipantRepository,
        IUserRepository userRepository,
        IRatingService ratingService)
    {
        _waitlistRepository = waitlistRepository;
        _gameRepository = gameRepository;
        _gameParticipantRepository = gameParticipantRepository;
        _userRepository = userRepository;
        _ratingService = ratingService;
    }

    public async Task<WaitlistResponse> JoinWaitlistAsync(int userId, int gameId)
    {
        // Verify game exists
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        // Check if game is full
        var participants = await _gameParticipantRepository.FindAsync(gp => gp.GameId == gameId);
        if (participants.Count() < game.MaxPlayers)
            throw new BusinessException("Game is not full. Join the game directly instead.");

        // Check if user is already in the game
        if (participants.Any(gp => gp.UserId == userId))
            throw new BusinessException("You are already in this game");

        // Check if already on waitlist
        var existingWaitlist = await _waitlistRepository.FindAsync(w =>
            w.GameId == gameId && w.UserId == userId);

        if (existingWaitlist.Any())
            throw new BusinessException("You are already on the waitlist for this game");

        // Check waitlist size
        var waitlistEntries = await _waitlistRepository.FindAsync(w => w.GameId == gameId);
        if (waitlistEntries.Count() >= MaxWaitlistSize)
            throw new BusinessException($"Waitlist is full (max {MaxWaitlistSize})");

        // Calculate position
        var position = waitlistEntries.Count() + 1;

        // Create waitlist entry
        var waitlistEntry = new Waitlist
        {
            GameId = gameId,
            UserId = userId,
            Position = position,
            JoinedAt = DateTime.UtcNow,
            IsInvited = false
        };

        await _waitlistRepository.AddAsync(waitlistEntry);
        await _waitlistRepository.SaveChangesAsync();

        // Re-sort waitlist by rating
        await ResortWaitlistAsync(gameId);

        return await MapToResponseAsync(waitlistEntry);
    }

    public async Task<IEnumerable<WaitlistResponse>> GetWaitlistAsync(int gameId)
    {
        var waitlistEntries = await _waitlistRepository.FindAsync(w => w.GameId == gameId);

        // Get user ratings for sorting
        var entriesWithRatings = new List<(Waitlist Entry, decimal Rating)>();

        foreach (var entry in waitlistEntries)
        {
            var rating = await _ratingService.GetAverageRatingAsync(playerId: entry.UserId);
            entriesWithRatings.Add((entry, rating));
        }

        // Sort by rating (highest first)
        var sortedEntries = entriesWithRatings
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Entry.JoinedAt)
            .Select(x => x.Entry)
            .ToList();

        // Update positions
        for (int i = 0; i < sortedEntries.Count; i++)
        {
            sortedEntries[i].Position = i + 1;
            await _waitlistRepository.UpdateAsync(sortedEntries[i]);
        }
        await _waitlistRepository.SaveChangesAsync();

        return await Task.WhenAll(sortedEntries.Select(MapToResponseAsync));
    }

    public async Task InviteFromWaitlistAsync(int gameOwnerId, int gameId, int waitlistUserId)
    {
        // Verify game exists and user is owner
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.CreatedBy != gameOwnerId)
            throw new UnauthorizedException("Only the game owner can invite from waitlist");

        // Find waitlist entry
        var waitlistEntries = await _waitlistRepository.FindAsync(w =>
            w.GameId == gameId && w.UserId == waitlistUserId);
        var waitlistEntry = waitlistEntries.FirstOrDefault();

        if (waitlistEntry == null)
            throw new NotFoundException("User not found on waitlist");

        // Mark as invited
        waitlistEntry.IsInvited = true;
        await _waitlistRepository.UpdateAsync(waitlistEntry);
        await _waitlistRepository.SaveChangesAsync();

        // In a real system, send notification here
    }

    public async Task RemoveFromWaitlistAsync(int userId, int gameId)
    {
        var waitlistEntries = await _waitlistRepository.FindAsync(w =>
            w.GameId == gameId && w.UserId == userId);
        var waitlistEntry = waitlistEntries.FirstOrDefault();

        if (waitlistEntry == null)
            throw new NotFoundException("Waitlist entry not found");

        await _waitlistRepository.DeleteAsync(waitlistEntry);
        await _waitlistRepository.SaveChangesAsync();

        // Re-sort remaining entries
        await ResortWaitlistAsync(gameId);
    }

    private async Task ResortWaitlistAsync(int gameId)
    {
        // Get and resort waitlist
        await GetWaitlistAsync(gameId);
    }

    private async Task<WaitlistResponse> MapToResponseAsync(Waitlist waitlist)
    {
        var user = await _userRepository.GetByIdAsync(waitlist.UserId);
        var game = await _gameRepository.GetByIdAsync(waitlist.GameId);
        var userRating = await _ratingService.GetAverageRatingAsync(playerId: waitlist.UserId);

        return new WaitlistResponse
        {
            WaitlistId = waitlist.WaitlistId,
            GameId = waitlist.GameId,
            GameTitle = game?.Title ?? "Unknown",
            UserId = waitlist.UserId,
            UserName = user?.FullName ?? "Unknown",
            UserRating = userRating,
            Position = waitlist.Position,
            JoinedAt = waitlist.JoinedAt,
            IsInvited = waitlist.IsInvited
        };
    }
}

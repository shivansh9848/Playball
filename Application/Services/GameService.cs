using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IRepository<GameParticipant> _participantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingService _bookingService;
    private readonly IWalletService _walletService;
    private readonly IRepository<Venue> _venueRepository;
    private readonly IBookingRepository _bookingRepository;

    public GameService(
        IGameRepository gameRepository,
        IRepository<GameParticipant> participantRepository,
        IUserRepository userRepository,
        IBookingService bookingService,
        IWalletService walletService,
        IRepository<Venue> venueRepository,
        IBookingRepository bookingRepository)
    {
        _gameRepository = gameRepository;
        _participantRepository = participantRepository;
        _userRepository = userRepository;
        _bookingService = bookingService;
        _walletService = walletService;
        _venueRepository = venueRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<GameResponse> CreateGameAsync(int userId, CreateGameRequest request)
    {
        if (request.MinPlayers > request.MaxPlayers)
            throw new BusinessException("MinPlayers cannot be greater than MaxPlayers");

        if (request.StartTime >= request.EndTime)
            throw new BusinessException("StartTime must be before EndTime");

        if (request.StartTime <= DateTime.UtcNow)
            throw new BusinessException("StartTime must be in the future");

        // Validate that the user has a confirmed booking for this slot
        var booking = await _bookingRepository.FirstOrDefaultAsync(b => 
            b.UserId == userId && 
            b.CourtId == request.CourtId && 
            b.SlotStartTime == request.StartTime && 
            b.SlotEndTime == request.EndTime &&
            b.Status == BookingStatus.Confirmed);

        if (booking == null)
            throw new BusinessException("You must have a confirmed booking for this slot to create a game.");

        // Check if a game already exists for this booking
        var existingGame = await _gameRepository.FirstOrDefaultAsync(g => g.BookingId == booking.BookingId);
        if (existingGame != null)
            throw new BusinessException("A game has already been created for this booking.");

        var game = new Game
        {
            Title = request.Title,
            Description = request.Description,
            VenueId = request.VenueId,
            CourtId = request.CourtId,
            CreatedBy = userId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            CurrentPlayers = 1,
            Status = GameStatus.Open,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow,
            BookingId = booking.BookingId
        };

        await _gameRepository.AddAsync(game);
        await _gameRepository.SaveChangesAsync();

        // Add creator as first participant
        var participant = new GameParticipant
        {
            GameId = game.GameId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            Status = ParticipantStatus.Accepted // Creator is always accepted
        };

        await _participantRepository.AddAsync(participant);
        await _participantRepository.SaveChangesAsync();

        // Auto-promote creator to GameOwner if they are a plain User
        var creator = await _userRepository.GetByIdAsync(userId);
        if (creator != null && creator.Role == UserRole.User)
        {
            creator.Role = UserRole.GameOwner;
            await _userRepository.UpdateAsync(creator);
            await _userRepository.SaveChangesAsync();
        }

        return await MapToResponse(game.GameId);
    }

    public async Task<GameResponse> JoinGameAsync(int userId, int gameId)
    {
        var game = await _gameRepository.GetGameWithParticipantsAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.Status != GameStatus.Open)
            throw new BusinessException("Game is not open for joining");

        if (game.Participants.Any(p => p.UserId == userId && p.IsActive))
            throw new BusinessException("You are already in this game");

        if (game.CurrentPlayers >= game.MaxPlayers)
            throw new BusinessException("Game is full");

        var status = game.IsPublic ? ParticipantStatus.Accepted : ParticipantStatus.Pending;

        var participant = new GameParticipant
        {
            GameId = gameId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            Status = status
        };

        await _participantRepository.AddAsync(participant);

        if (status == ParticipantStatus.Accepted)
        {
            game.CurrentPlayers++;
            if (game.CurrentPlayers >= game.MaxPlayers)
                game.Status = GameStatus.Full;

            await _gameRepository.UpdateAsync(game);
        }
        
        await _gameRepository.SaveChangesAsync();

        return await MapToResponse(gameId);
    }

    public async Task<GameResponse> LeaveGameAsync(int userId, int gameId)
    {
        var game = await _gameRepository.GetGameWithParticipantsAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        var participant = game.Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);
        if (participant == null)
            throw new BusinessException("You are not in this game");

        if (game.CreatedBy == userId)
            throw new BusinessException("Game creator cannot leave the game");

        participant.IsActive = false;
        await _participantRepository.UpdateAsync(participant);

        if (participant.Status == ParticipantStatus.Accepted)
        {
            game.CurrentPlayers--;
            if (game.Status == GameStatus.Full && game.CurrentPlayers < game.MaxPlayers)
                game.Status = GameStatus.Open;
            
            await _gameRepository.UpdateAsync(game);
        }

        await _gameRepository.SaveChangesAsync();

        return await MapToResponse(gameId);
    }

    public async Task<GameResponse> ApproveParticipantAsync(int userId, int gameId, int participantId)
    {
        var game = await _gameRepository.GetGameWithParticipantsAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.CreatedBy != userId)
            throw new UnauthorizedException("Only the game owner can approve participants");

        var participant = await _participantRepository.GetByIdAsync(participantId);
        if (participant == null || participant.GameId != gameId)
            throw new NotFoundException("Participant", participantId);

        if (participant.Status != ParticipantStatus.Pending)
            throw new BusinessException("Participant is not pending approval");

        if (game.CurrentPlayers >= game.MaxPlayers)
            throw new BusinessException("Game is full");

        participant.Status = ParticipantStatus.Accepted;
        await _participantRepository.UpdateAsync(participant);

        game.CurrentPlayers++;
        if (game.CurrentPlayers >= game.MaxPlayers)
            game.Status = GameStatus.Full;

        await _gameRepository.UpdateAsync(game);
        await _gameRepository.SaveChangesAsync();

        return await MapToResponse(gameId);
    }

    public async Task<IEnumerable<GameResponse>> GetPublicGamesAsync()
    {
        var games = await _gameRepository.GetPublicGamesAsync();
        var response = new List<GameResponse>();
        foreach (var game in games)
        {
            response.Add(await MapToResponse(game.GameId));
        }
        return response;
    }

    public async Task<IEnumerable<GameResponse>> GetMyGamesAsync(int userId)
    {
        var games = await _gameRepository.GetGamesByUserAsync(userId);
        var response = new List<GameResponse>();
        foreach (var game in games)
        {
            response.Add(await MapToResponse(game.GameId));
        }
        return response;
    }

    public async Task<GameResponse?> GetGameByIdAsync(int gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        return game != null ? await MapToResponse(gameId) : null;
    }

    public async Task AutoCancelGamesAsync()
    {
        var now = DateTime.UtcNow;
        var games = await _gameRepository.FindAsync(g =>
            (g.Status == GameStatus.Open || g.Status == GameStatus.Full) &&
            g.StartTime <= now &&
            g.CurrentPlayers < g.MinPlayers);

        foreach (var game in games)
        {
            game.Status = GameStatus.Cancelled;
            await _gameRepository.UpdateAsync(game);

            // Cancel the booking and process refund
            if (game.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdAsync(game.BookingId.Value);
                if (booking != null && booking.Status != BookingStatus.Cancelled)
                {
                    await _bookingService.CancelBookingAsync(booking.UserId, booking.BookingId, "Game Cancelled (Min players not met)");
                }
            }
        }
        await _gameRepository.SaveChangesAsync();
    }

    public async Task CompleteGameAsync(int gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null) throw new NotFoundException("Game", gameId);

        if (game.Status != GameStatus.Full && game.Status != GameStatus.Open) // Can we complete open games? Probably if they met min players.
             throw new BusinessException("Game cannot be completed in its current status");

        game.Status = GameStatus.Completed;
        game.CompletedAt = DateTime.UtcNow;
        await _gameRepository.UpdateAsync(game);

        if (game.BookingId.HasValue)
        {
             // Payout logic
             var booking = await _bookingRepository.GetByIdAsync(game.BookingId.Value);
             if (booking != null)
             {
                 var venue = await _venueRepository.GetByIdAsync(game.VenueId);
                 if (venue != null)
                 {
                     await _walletService.CreditWalletAsync(
                         venue.OwnerId, 
                         booking.AmountPaid, 
                         $"Payout for Game #{game.GameId} (Booking #{booking.BookingId})",
                         $"PAYOUT-{game.GameId}");
                 }
             }
        }
        await _gameRepository.SaveChangesAsync();
    }

    private async Task<GameResponse> MapToResponse(int gameId)
    {
        var game = await _gameRepository.GetGameWithParticipantsAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        return new GameResponse
        {
            GameId = game.GameId,
            Title = game.Title,
            Description = game.Description,
            VenueId = game.VenueId,
            VenueName = "", // Will be set if needed
            CourtId = game.CourtId,
            CourtName = "", // Will be set if needed
            CreatedBy = game.CreatedBy,
            CreatorName = game.Creator.FullName,
            StartTime = game.StartTime,
            EndTime = game.EndTime,
            MinPlayers = game.MinPlayers,
            MaxPlayers = game.MaxPlayers,
            CurrentPlayers = game.CurrentPlayers,
            Status = game.Status.ToString(),
            IsPublic = game.IsPublic,
            CreatedAt = game.CreatedAt,
            Participants = game.Participants
                .Where(p => p.IsActive)
                .Select(p => new ParticipantResponse
                {
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    Rating = p.User.AggregatedRating
                }).ToList()
        };
    }
}

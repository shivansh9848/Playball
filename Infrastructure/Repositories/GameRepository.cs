using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Data;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Game>> GetPublicGamesAsync()
    {
        return await _dbSet
            .Include(g => g.Creator)
            .Include(g => g.Participants)
                .ThenInclude(p => p.User)
            .Where(g => g.IsPublic && g.Status == GameStatus.Open)
            .OrderBy(g => g.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetGamesByUserAsync(int userId)
    {
        return await _dbSet
            .Include(g => g.Creator)
            .Include(g => g.Participants)
                .ThenInclude(p => p.User)
            .Where(g => g.CreatedBy == userId || g.Participants.Any(p => p.UserId == userId && p.IsActive))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Game?> GetGameWithParticipantsAsync(int gameId)
    {
        return await _dbSet
            .Include(g => g.Creator)
            .Include(g => g.Participants)
                .ThenInclude(p => p.User)
            .Include(g => g.WaitlistEntries)
                .ThenInclude(w => w.User)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<IEnumerable<Game>> GetGamesRequiringCancellationAsync()
    {
        var threshold = DateTime.UtcNow.AddMinutes(5);
        return await _dbSet
            .Include(g => g.Participants)
            .Where(g => g.Status == GameStatus.Open
                && g.StartTime <= threshold
                && g.Participants.Count(p => p.IsActive) < g.MinPlayers)
            .ToListAsync();
    }
}

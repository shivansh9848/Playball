using Assignment_Example_HU.Domain.Entities;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public interface IGameRepository : IRepository<Game>
{
    Task<IEnumerable<Game>> GetPublicGamesAsync();
    Task<IEnumerable<Game>> GetGamesByUserAsync(int userId);
    Task<Game?> GetGameWithParticipantsAsync(int gameId);
    Task<IEnumerable<Game>> GetGamesRequiringCancellationAsync();
}

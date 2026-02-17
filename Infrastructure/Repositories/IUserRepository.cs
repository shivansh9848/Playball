using Assignment_Example_HU.Domain.Entities;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithWalletAsync(int userId);
}

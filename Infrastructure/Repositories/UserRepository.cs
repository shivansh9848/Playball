using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Data;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdWithWalletAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}

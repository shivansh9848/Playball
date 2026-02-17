using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Data;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public class VenueRepository : Repository<Venue>, IVenueRepository
{
    public VenueRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Venue>> GetVenuesByOwnerAsync(int ownerId)
    {
        return await _dbSet
            .Include(v => v.Courts)
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venue>> GetApprovedVenuesAsync()
    {
        return await _dbSet
            .Include(v => v.Courts)
            .Where(v => v.ApprovalStatus == ApprovalStatus.Approved)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venue>> GetPendingVenuesAsync()
    {
        return await _dbSet
            .Include(v => v.Owner)
            .Where(v => v.ApprovalStatus == ApprovalStatus.Pending)
            .ToListAsync();
    }

    public async Task<Venue?> GetVenueWithCourtsAsync(int venueId)
    {
        return await _dbSet
            .Include(v => v.Courts)
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.VenueId == venueId);
    }
}

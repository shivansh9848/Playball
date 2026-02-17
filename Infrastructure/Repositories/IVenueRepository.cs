using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public interface IVenueRepository : IRepository<Venue>
{
    Task<IEnumerable<Venue>> GetVenuesByOwnerAsync(int ownerId);
    Task<IEnumerable<Venue>> GetApprovedVenuesAsync();
    Task<IEnumerable<Venue>> GetPendingVenuesAsync();
    Task<Venue?> GetVenueWithCourtsAsync(int venueId);
}

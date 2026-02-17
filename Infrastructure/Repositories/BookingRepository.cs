using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Data;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId)
    {
        return await _dbSet
            .Include(b => b.Court)
                .ThenInclude(c => c.Venue)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCourtAsync(int courtId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.CourtId == courtId
                && b.SlotStartTime >= startDate
                && b.SlotEndTime <= endDate
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Locked))
            .ToListAsync();
    }

    public async Task<bool> IsSlotAvailableAsync(int courtId, DateTime slotStart, DateTime slotEnd)
    {
        var conflictingBooking = await _dbSet
            .AnyAsync(b => b.CourtId == courtId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Locked)
                && b.SlotStartTime < slotEnd
                && b.SlotEndTime > slotStart);

        return !conflictingBooking;
    }

    public async Task<IEnumerable<Booking>> GetExpiredLocksAsync()
    {
        return await _dbSet
            .Where(b => b.Status == BookingStatus.Locked
                && b.LockExpiryTime.HasValue
                && b.LockExpiryTime.Value < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsForRefundAsync()
    {
        return await _dbSet
            .Include(b => b.User)
                .ThenInclude(u => u.Wallet)
            .Where(b => b.Status == BookingStatus.Cancelled
                && b.AmountPaid > 0)
            .ToListAsync();
    }
}

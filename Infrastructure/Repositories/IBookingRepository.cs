using Assignment_Example_HU.Domain.Entities;

namespace Assignment_Example_HU.Infrastructure.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId);
    Task<IEnumerable<Booking>> GetBookingsByCourtAsync(int courtId, DateTime startDate, DateTime endDate);
    Task<bool> IsSlotAvailableAsync(int courtId, DateTime slotStart, DateTime slotEnd);
    Task<IEnumerable<Booking>> GetExpiredLocksAsync();
    Task<IEnumerable<Booking>> GetBookingsForRefundAsync();
}

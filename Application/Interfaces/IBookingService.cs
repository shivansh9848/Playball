using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> LockSlotAsync(int userId, LockSlotRequest request);
    Task<BookingResponse> ConfirmBookingAsync(int userId, ConfirmBookingRequest request);
    Task<BookingResponse> CancelBookingAsync(int userId, int bookingId, string? reason);
    Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int userId);
    Task<BookingResponse?> GetBookingByIdAsync(int bookingId);
    Task ExpirePendingBookingsAsync();
}

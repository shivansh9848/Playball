using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Constants;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Caching;
using Assignment_Example_HU.Infrastructure.Data;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<Court> _courtRepository;
    private readonly IRepository<Venue> _venueRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IPricingService _pricingService;
    private readonly ICacheService _cacheService;
    private readonly ApplicationDbContext _context;

    public BookingService(
        IBookingRepository bookingRepository,
        IRepository<Court> courtRepository,
        IRepository<Venue> venueRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Transaction> transactionRepository,
        IPricingService pricingService,
        ICacheService cacheService,
        ApplicationDbContext context)
    {
        _bookingRepository = bookingRepository;
        _courtRepository = courtRepository;
        _venueRepository = venueRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _pricingService = pricingService;
        _cacheService = cacheService;
        _context = context;
    }

    public async Task<BookingResponse> LockSlotAsync(int userId, LockSlotRequest request)
    {
        // Validate court exists
        var court = await _courtRepository.GetByIdAsync(request.CourtId);
        if (court == null)
            throw new NotFoundException("Court", request.CourtId);

        // Validate slot time
        if (request.SlotStartTime <= DateTime.UtcNow)
            throw new BusinessException("Slot start time must be in the future");

        if (request.SlotEndTime <= request.SlotStartTime)
            throw new BusinessException("Slot end time must be after start time");

        // Validate slot duration is valid
        var duration = (request.SlotEndTime - request.SlotStartTime).TotalMinutes;
        if (duration < court.SlotDurationMinutes)
            throw new BusinessException($"Minimum slot duration is {court.SlotDurationMinutes} minutes");

        // Check if slot is within operating hours
        if (!IsSlotWithinOperatingHours(court, request.SlotStartTime, request.SlotEndTime))
            throw new BusinessException("Slot is outside court operating hours");

        // Create distributed lock key
        var lockKey = $"slot_lock:{request.CourtId}:{request.SlotStartTime:yyyyMMddHHmmss}";

        // Try to acquire lock
        var lockAcquired = await _cacheService.SetIfNotExistsAsync(lockKey, userId, TimeSpan.FromMinutes(5));
        if (!lockAcquired)
        {
            throw new BusinessException("This slot is currently being booked by another user. Please try again in a few moments.");
        }

        // Check if slot is available
        var isAvailable = await _bookingRepository.IsSlotAvailableAsync(
            request.CourtId,
            request.SlotStartTime,
            request.SlotEndTime
        );

        if (!isAvailable)
        {
            // Release lock
            await _cacheService.RemoveAsync(lockKey);
            throw new BusinessException("This slot is already booked");
        }

        // Calculate price
        var pricingBreakdown = await _pricingService.GetPricingBreakdownAsync(
            request.CourtId,
            request.SlotStartTime,
            request.SlotEndTime
        );

        // Create booking in pending status with locked price
        var booking = new Booking
        {
            CourtId = request.CourtId,
            UserId = userId,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            Status = BookingStatus.Pending,
            PriceLocked = pricingBreakdown.FinalPrice,
            LockExpiryTime = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        // Release the cache lock after creating the booking
        await _cacheService.RemoveAsync(lockKey);

        return await MapToResponseAsync(booking);
    }

    public async Task<BookingResponse> ConfirmBookingAsync(int userId, ConfirmBookingRequest request)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new NotFoundException("Booking", request.BookingId);

        if (booking.UserId != userId)
            throw new UnauthorizedException("You can only confirm your own bookings");

        if (booking.Status != BookingStatus.Pending)
            throw new BusinessException("Only pending bookings can be confirmed");

        // Check if lock has expired
        if (booking.LockExpiryTime.HasValue && booking.LockExpiryTime.Value < DateTime.UtcNow)
        {
            // Cancel the expired booking
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Lock expired";
            booking.CancelledAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            throw new BusinessException("Booking lock has expired. Please create a new booking.");
        }

        // Verify slot is still available (double-check - exclude current booking)
        var overlappingBookings = await _bookingRepository.FindAsync(b =>
            b.CourtId == booking.CourtId &&
            b.BookingId != booking.BookingId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending) &&
            b.SlotStartTime < booking.SlotEndTime &&
            b.SlotEndTime > booking.SlotStartTime
        );

        if (overlappingBookings.Any())
            throw new BusinessException("This slot has been booked by someone else");

        // Get user wallet
        var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
        var userWallet = wallets.FirstOrDefault();

        if (userWallet == null)
            throw new NotFoundException("Wallet not found for user");

        if (userWallet.Balance < booking.PriceLocked)
            throw new BusinessException("Insufficient wallet balance");

        // ACID Transaction: Wrap wallet debit + booking confirm in database transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Debit wallet
            userWallet.Balance -= booking.PriceLocked;
            userWallet.UpdatedAt = DateTime.UtcNow;
            await _walletRepository.UpdateAsync(userWallet);

            // Create transaction record
            var walletTransaction = new Transaction
            {
                WalletId = userWallet.WalletId,
                Type = TransactionType.Debit,
                Amount = booking.PriceLocked,
                BalanceAfter = userWallet.Balance,
                Description = $"Booking for Court #{booking.CourtId} on {booking.SlotStartTime:yyyy-MM-dd HH:mm}",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(walletTransaction);

            // Update booking
            booking.Status = BookingStatus.Confirmed;
            booking.AmountPaid = booking.PriceLocked;
            booking.ConfirmedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);

            // Commit all changes atomically
            await _bookingRepository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            // Rollback on any error
            await transaction.RollbackAsync();
            throw;
        }

        return await MapToResponseAsync(booking);
    }

    public async Task<BookingResponse> CancelBookingAsync(int userId, int bookingId, string? reason)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
            throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId)
            throw new UnauthorizedException("You can only cancel your own bookings");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessException("Booking is already cancelled");

        if (booking.Status == BookingStatus.Completed)
            throw new BusinessException("Cannot cancel a completed booking");

        // Calculate refund
        var refundPercentage = CalculateRefundPercentage(booking.SlotStartTime);
        var refundAmount = booking.AmountPaid * refundPercentage / 100;

        // Update booking status
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.CancelledAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        // Process refund if applicable
        if (refundAmount > 0)
        {
            var wallets = await _walletRepository.FindAsync(w => w.UserId == userId);
            var userWallet = wallets.FirstOrDefault();

            if (userWallet != null)
            {
                userWallet.Balance += refundAmount;
                await _walletRepository.UpdateAsync(userWallet);

                // Create transaction record
                var transaction = new Transaction
                {
                    WalletId = userWallet.WalletId,
                    Type = TransactionType.Credit,
                    Amount = refundAmount,
                    BalanceAfter = userWallet.Balance,
                    Description = $"Refund for cancelled booking #{bookingId} ({refundPercentage}%)",
                    BookingId = bookingId,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(transaction);
            }
        }

        await _bookingRepository.SaveChangesAsync();

        return await MapToResponseAsync(booking);
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
            return null;

        return await MapToResponseAsync(booking);
    }

    public async Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int userId)
    {
        var bookings = await _bookingRepository.FindAsync(b => b.UserId == userId);
        var responses = new List<BookingResponse>();

        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return responses;
    }

    public async Task ExpirePendingBookingsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredBookings = await _bookingRepository.FindAsync(b =>
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Locked) &&
            b.LockExpiryTime.HasValue &&
            b.LockExpiryTime.Value <= now);

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Expired;
            await _bookingRepository.UpdateAsync(booking);
        }
        await _bookingRepository.SaveChangesAsync();
    }

    private bool IsSlotWithinOperatingHours(Court court, DateTime slotStart, DateTime slotEnd)
    {
        var openTime = TimeSpan.Parse(court.OpenTime);
        var closeTime = TimeSpan.Parse(court.CloseTime);

        var slotStartTime = slotStart.TimeOfDay;
        var slotEndTime = slotEnd.TimeOfDay;

        return slotStartTime >= openTime && slotEndTime <= closeTime;
    }

    private decimal CalculateRefundPercentage(DateTime slotStartTime)
    {
        var hoursUntilSlot = (slotStartTime - DateTime.UtcNow).TotalHours;

        if (hoursUntilSlot > RefundConstants.FullRefundWindowHours)
            return RefundConstants.RefundPercent_MoreThan24Hours;
        if (hoursUntilSlot >= RefundConstants.PartialRefundWindowHours)
            return RefundConstants.RefundPercent_6To24Hours;

        return RefundConstants.RefundPercent_LessThan6Hours;
    }

    private async Task<BookingResponse> MapToResponseAsync(Booking booking)
    {
        // Load related entities
        var court = await _courtRepository.GetByIdAsync(booking.CourtId);
        var venue = court != null ? await _venueRepository.GetByIdAsync(court.VenueId) : null;

        return new BookingResponse
        {
            BookingId = booking.BookingId,
            CourtId = booking.CourtId,
            UserId = booking.UserId,
            CourtName = court?.Name ?? "",
            VenueName = venue?.Name ?? "",
            SlotStartTime = booking.SlotStartTime,
            SlotEndTime = booking.SlotEndTime,
            Status = booking.Status.ToString(),
            PriceLocked = booking.PriceLocked,
            AmountPaid = booking.AmountPaid,
            LockExpiryTime = booking.LockExpiryTime,
            CreatedAt = booking.CreatedAt,
            ConfirmedAt = booking.ConfirmedAt,
            CancelledAt = booking.CancelledAt,
            CancellationReason = booking.CancellationReason
        };
    }
}

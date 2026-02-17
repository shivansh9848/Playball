using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class CourtService : ICourtService
{
    private readonly IRepository<Court> _courtRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IBookingRepository _bookingRepository;

    public CourtService(
        IRepository<Court> courtRepository,
        IVenueRepository venueRepository,
        IBookingRepository bookingRepository)
    {
        _courtRepository = courtRepository;
        _venueRepository = venueRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<CourtResponse> CreateCourtAsync(int ownerId, CreateCourtRequest request)
    {
        var venue = await _venueRepository.GetByIdAsync(request.VenueId);
        if (venue == null)
            throw new NotFoundException("Venue", request.VenueId);

        if (venue.OwnerId != ownerId)
            throw new UnauthorizedException("You can only create courts for your own venues");

        if (venue.ApprovalStatus != ApprovalStatus.Approved)
            throw new BusinessException("Venue must be approved before adding courts");

        var court = new Court
        {
            VenueId = request.VenueId,
            Name = request.Name,
            SportType = (SportType)request.SportType,
            SlotDurationMinutes = request.SlotDurationMinutes,
            BasePrice = request.BasePrice,
            OpenTime = request.OpenTime,
            CloseTime = request.CloseTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _courtRepository.AddAsync(court);
        await _courtRepository.SaveChangesAsync();

        return MapToResponse(court, venue);
    }

    public async Task<CourtResponse> UpdateCourtAsync(int ownerId, int courtId, UpdateCourtRequest request)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        var venue = await _venueRepository.GetByIdAsync(court.VenueId);
        if (venue == null || venue.OwnerId != ownerId)
            throw new UnauthorizedException("You can only update courts in your own venues");

        if (request.Name != null) court.Name = request.Name;
        if (request.SlotDurationMinutes.HasValue) court.SlotDurationMinutes = request.SlotDurationMinutes.Value;
        if (request.BasePrice.HasValue) court.BasePrice = request.BasePrice.Value;
        if (request.OpenTime != null) court.OpenTime = request.OpenTime;
        if (request.CloseTime != null) court.CloseTime = request.CloseTime;
        if (request.IsActive.HasValue) court.IsActive = request.IsActive.Value;

        await _courtRepository.UpdateAsync(court);
        await _courtRepository.SaveChangesAsync();

        return MapToResponse(court, venue);
    }

    public async Task DeleteCourtAsync(int ownerId, int courtId)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        var venue = await _venueRepository.GetByIdAsync(court.VenueId);
        if (venue == null || venue.OwnerId != ownerId)
            throw new UnauthorizedException("You can only delete courts in your own venues");

        // Check for future bookings
        var futureBookings = await _bookingRepository.FindAsync(b =>
            b.CourtId == courtId &&
            b.SlotStartTime > DateTime.UtcNow &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Locked));

        if (futureBookings.Any())
            throw new BusinessException("Cannot delete court with future bookings");

        await _courtRepository.DeleteAsync(court);
        await _courtRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<CourtResponse>> GetCourtsByVenueAsync(int venueId)
    {
        var venue = await _venueRepository.GetVenueWithCourtsAsync(venueId);
        if (venue == null)
            throw new NotFoundException("Venue", venueId);

        return venue.Courts.Select(c => MapToResponse(c, venue));
    }

    public async Task<CourtResponse?> GetCourtByIdAsync(int courtId)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            return null;

        var venue = await _venueRepository.GetByIdAsync(court.VenueId);
        return venue != null ? MapToResponse(court, venue) : null;
    }

    private CourtResponse MapToResponse(Court court, Venue venue)
    {
        return new CourtResponse
        {
            CourtId = court.CourtId,
            VenueId = court.VenueId,
            VenueName = venue.Name,
            Name = court.Name,
            SportType = court.SportType.ToString(),
            SlotDurationMinutes = court.SlotDurationMinutes,
            BasePrice = court.BasePrice,
            OpenTime = court.OpenTime,
            CloseTime = court.CloseTime,
            IsActive = court.IsActive,
            CreatedAt = court.CreatedAt
        };
    }
}

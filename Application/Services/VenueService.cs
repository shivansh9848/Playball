using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUserRepository _userRepository;

    public VenueService(IVenueRepository venueRepository, IUserRepository userRepository)
    {
        _venueRepository = venueRepository;
        _userRepository = userRepository;
    }

    public async Task<VenueResponse> CreateVenueAsync(int ownerId, CreateVenueRequest request)
    {
        var owner = await _userRepository.GetByIdAsync(ownerId);
        if (owner == null)
            throw new NotFoundException("Owner", ownerId);

        if (owner.Role != UserRole.VenueOwner && owner.Role != UserRole.Admin)
            throw new BusinessException("Only venue owners can create venues");

        var venue = new Venue
        {
            Name = request.Name,
            Address = request.Address,
            SportsSupported = string.Join(",", request.SportsSupported),
            OwnerId = ownerId,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _venueRepository.AddAsync(venue);
        await _venueRepository.SaveChangesAsync();

        return await MapToResponse(venue);
    }

    public async Task<VenueResponse> ApproveVenueAsync(int adminId, int venueId, ApproveVenueRequest request)
    {
        var admin = await _userRepository.GetByIdAsync(adminId);
        if (admin == null || admin.Role != UserRole.Admin)
            throw new UnauthorizedException("Only admins can approve venues");

        var venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null)
            throw new NotFoundException("Venue", venueId);

        if (request.ApprovalStatus != 2 && request.ApprovalStatus != 3)
            throw new BusinessException("Invalid approval status");

        venue.ApprovalStatus = (ApprovalStatus)request.ApprovalStatus;
        venue.ApprovedAt = DateTime.UtcNow;
        venue.ApprovedBy = adminId;
        venue.RejectionReason = request.RejectionReason;

        await _venueRepository.UpdateAsync(venue);
        await _venueRepository.SaveChangesAsync();

        return await MapToResponse(venue);
    }

    public async Task<IEnumerable<VenueResponse>> GetAllVenuesAsync()
    {
        var venues = await _venueRepository.GetApprovedVenuesAsync();
        return await Task.WhenAll(venues.Select(MapToResponse));
    }

    public async Task<IEnumerable<VenueResponse>> GetMyVenuesAsync(int ownerId)
    {
        var venues = await _venueRepository.GetVenuesByOwnerAsync(ownerId);
        return await Task.WhenAll(venues.Select(MapToResponse));
    }

    public async Task<IEnumerable<VenueResponse>> GetAllVenuesAsync(string? sportsSupported = null, string? location = null)
    {
        var query = await _venueRepository.FindAsync(v => v.ApprovalStatus == ApprovalStatus.Approved);
        
        // Apply optional filters
        if (!string.IsNullOrEmpty(sportsSupported))
        {
            query = query.Where(v => v.SportsSupported != null && v.SportsSupported.Contains(sportsSupported, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(v => v.Address != null && v.Address.Contains(location, StringComparison.OrdinalIgnoreCase));
        }
        
        return await Task.WhenAll(query.Select(MapToResponse));
    }

    public async Task<IEnumerable<VenueResponse>> GetPendingVenuesAsync()
    {
        var venues = await _venueRepository.GetPendingVenuesAsync();
        return await Task.WhenAll(venues.Select(MapToResponse));
    }

    public async Task<VenueResponse?> GetVenueByIdAsync(int venueId)
    {
        var venue = await _venueRepository.GetVenueWithCourtsAsync(venueId);
        return venue != null ? await MapToResponse(venue) : null;
    }

    private async Task<VenueResponse> MapToResponse(Venue venue)
    {
        var owner = venue.Owner ?? await _userRepository.GetByIdAsync(venue.OwnerId);

        return new VenueResponse
        {
            VenueId = venue.VenueId,
            Name = venue.Name,
            Address = venue.Address,
            SportsSupported = venue.SportsSupported,
            OwnerId = venue.OwnerId,
            OwnerName = owner?.FullName ?? "Unknown",
            ApprovalStatus = venue.ApprovalStatus.ToString(),
            CreatedAt = venue.CreatedAt,
            ApprovedAt = venue.ApprovedAt,
            RejectionReason = venue.RejectionReason,
            Courts = venue.Courts?.Select(c => new CourtResponse
            {
                CourtId = c.CourtId,
                VenueId = c.VenueId,
                VenueName = venue.Name,
                Name = c.Name,
                SportType = c.SportType.ToString(),
                SlotDurationMinutes = c.SlotDurationMinutes,
                BasePrice = c.BasePrice,
                OpenTime = c.OpenTime,
                CloseTime = c.CloseTime,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList() ?? new List<CourtResponse>()
        };
    }
}

using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class CourtServiceTests
{
    private readonly Mock<IRepository<Court>> _courtRepoMock;
    private readonly Mock<IVenueRepository> _venueRepoMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly CourtService _courtService;

    public CourtServiceTests()
    {
        _courtRepoMock = new Mock<IRepository<Court>>();
        _venueRepoMock = new Mock<IVenueRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _courtService = new CourtService(_courtRepoMock.Object, _venueRepoMock.Object, _bookingRepoMock.Object);
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldCreate_WhenOwnerAndApproved()
    {
        var venue = new Venue { VenueId = 1, OwnerId = 1, ApprovalStatus = ApprovalStatus.Approved, Name = "Arena" };
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateCourtRequest 
        { 
            VenueId = 1, Name = "Court A", SportType = 1, 
            SlotDurationMinutes = 60, BasePrice = 500m, OpenTime = "06:00", CloseTime = "23:00" 
        };

        var result = await _courtService.CreateCourtAsync(1, request);

        Assert.Equal("Court A", result.Name);
        _courtRepoMock.Verify(r => r.AddAsync(It.IsAny<Court>()), Times.Once);
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldThrow_WhenVenueNotFound()
    {
        _venueRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);

        var request = new CreateCourtRequest { VenueId = 99, Name = "Court A" };
        await Assert.ThrowsAsync<NotFoundException>(() => _courtService.CreateCourtAsync(1, request));
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldThrow_WhenNotOwner()
    {
        var venue = new Venue { VenueId = 1, OwnerId = 2, ApprovalStatus = ApprovalStatus.Approved };
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateCourtRequest { VenueId = 1, Name = "Court A" };
        await Assert.ThrowsAsync<UnauthorizedException>(() => _courtService.CreateCourtAsync(1, request));
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldThrow_WhenVenueNotApproved()
    {
        var venue = new Venue { VenueId = 1, OwnerId = 1, ApprovalStatus = ApprovalStatus.Pending };
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateCourtRequest { VenueId = 1, Name = "Court A" };
        await Assert.ThrowsAsync<BusinessException>(() => _courtService.CreateCourtAsync(1, request));
    }

    [Fact]
    public async Task UpdateCourtAsync_ShouldUpdate_WhenOwner()
    {
        var court = new Court { CourtId = 1, VenueId = 1, Name = "Old Name" };
        var venue = new Venue { VenueId = 1, OwnerId = 1, Name = "Arena" };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new UpdateCourtRequest { Name = "New Name", BasePrice = 600m };
        var result = await _courtService.UpdateCourtAsync(1, 1, request);

        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task DeleteCourtAsync_ShouldDelete_WhenNoFutureBookings()
    {
        var court = new Court { CourtId = 1, VenueId = 1 };
        var venue = new Venue { VenueId = 1, OwnerId = 1 };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);
        _bookingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(new List<Booking>());

        await _courtService.DeleteCourtAsync(1, 1);

        _courtRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Court>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCourtAsync_ShouldThrow_WhenFutureBookingsExist()
    {
        var court = new Court { CourtId = 1, VenueId = 1 };
        var venue = new Venue { VenueId = 1, OwnerId = 1 };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);
        _bookingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(new List<Booking> { new() { BookingId = 1 } });

        await Assert.ThrowsAsync<BusinessException>(() => _courtService.DeleteCourtAsync(1, 1));
    }

    [Fact]
    public async Task DeleteCourtAsync_ShouldThrow_WhenNotOwner()
    {
        var court = new Court { CourtId = 1, VenueId = 1 };
        var venue = new Venue { VenueId = 1, OwnerId = 2 };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _courtService.DeleteCourtAsync(1, 1));
    }

    [Fact]
    public async Task GetCourtsByVenueAsync_ShouldReturnCourts()
    {
        var venue = new Venue 
        { 
            VenueId = 1, Name = "Arena", OwnerId = 1,
            Courts = new List<Court> 
            { 
                new() { CourtId = 1, Name = "Court A", VenueId = 1 }, 
                new() { CourtId = 2, Name = "Court B", VenueId = 1 } 
            } 
        };
        _venueRepoMock.Setup(r => r.GetVenueWithCourtsAsync(1)).ReturnsAsync(venue);

        var result = await _courtService.GetCourtsByVenueAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCourtByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _courtRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Court?)null);

        var result = await _courtService.GetCourtByIdAsync(99);

        Assert.Null(result);
    }
}

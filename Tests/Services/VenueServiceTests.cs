using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class VenueServiceTests
{
    private readonly Mock<IVenueRepository> _venueRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly VenueService _venueService;

    public VenueServiceTests()
    {
        _venueRepoMock = new Mock<IVenueRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _venueService = new VenueService(_venueRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldCreateVenue_WhenOwnerValid()
    {
        var owner = new User { UserId = 1, FullName = "Owner", Role = UserRole.VenueOwner };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(owner);
        _venueRepoMock.Setup(r => r.AddAsync(It.IsAny<Venue>())).ReturnsAsync(new Venue { Name = "Grand Arena", OwnerId = 1, Owner = owner });

        var request = new CreateVenueRequest { Name = "Grand Arena", Address = "123 Main Street, City", SportsSupported = new List<int> { 1, 2 } };
        var result = await _venueService.CreateVenueAsync(1, request);

        _venueRepoMock.Verify(r => r.AddAsync(It.IsAny<Venue>()), Times.Once);
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldThrow_WhenOwnerNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var request = new CreateVenueRequest { Name = "Arena", Address = "123 Main Street, City" };
        await Assert.ThrowsAsync<NotFoundException>(() => _venueService.CreateVenueAsync(99, request));
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldThrow_WhenNotVenueOwnerOrAdmin()
    {
        var user = new User { UserId = 1, FullName = "Player", Role = UserRole.User };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var request = new CreateVenueRequest { Name = "My Arena", Address = "123 Main Street, City" };
        await Assert.ThrowsAsync<BusinessException>(() => _venueService.CreateVenueAsync(1, request));
    }

    [Fact]
    public async Task ApproveVenueAsync_ShouldApprove_WhenAdminValid()
    {
        var admin = new User { UserId = 1, FullName = "Admin", Role = UserRole.Admin };
        var venue = new Venue { VenueId = 1, Name = "Arena", OwnerId = 2, ApprovalStatus = ApprovalStatus.Pending };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new ApproveVenueRequest { ApprovalStatus = 2 };
        var result = await _venueService.ApproveVenueAsync(1, 1, request);

        _venueRepoMock.Verify(r => r.UpdateAsync(It.Is<Venue>(v => v.ApprovalStatus == ApprovalStatus.Approved)), Times.Once);
    }

    [Fact]
    public async Task ApproveVenueAsync_ShouldThrow_WhenNotAdmin()
    {
        var user = new User { UserId = 1, FullName = "User", Role = UserRole.User };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var request = new ApproveVenueRequest { ApprovalStatus = 2 };
        await Assert.ThrowsAsync<UnauthorizedException>(() => _venueService.ApproveVenueAsync(1, 1, request));
    }

    [Fact]
    public async Task ApproveVenueAsync_ShouldThrow_WhenVenueNotFound()
    {
        var admin = new User { UserId = 1, FullName = "Admin", Role = UserRole.Admin };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _venueRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);

        var request = new ApproveVenueRequest { ApprovalStatus = 2 };
        await Assert.ThrowsAsync<NotFoundException>(() => _venueService.ApproveVenueAsync(1, 99, request));
    }

    [Fact]
    public async Task ApproveVenueAsync_ShouldThrow_WhenInvalidStatus()
    {
        var admin = new User { UserId = 1, FullName = "Admin", Role = UserRole.Admin };
        var venue = new Venue { VenueId = 1, Name = "Arena", OwnerId = 2 };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new ApproveVenueRequest { ApprovalStatus = 99 };
        await Assert.ThrowsAsync<BusinessException>(() => _venueService.ApproveVenueAsync(1, 1, request));
    }

    [Fact]
    public async Task GetVenueByIdAsync_ShouldReturnVenue()
    {
        var venue = new Venue { VenueId = 1, Name = "Arena", OwnerId = 1, Owner = new User { UserId = 1, FullName = "Owner" }, Courts = new List<Court>() };
        _venueRepoMock.Setup(r => r.GetVenueWithCourtsAsync(1)).ReturnsAsync(venue);

        var result = await _venueService.GetVenueByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Arena", result!.Name);
    }

    [Fact]
    public async Task GetVenueByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _venueRepoMock.Setup(r => r.GetVenueWithCourtsAsync(99)).ReturnsAsync((Venue?)null);

        var result = await _venueService.GetVenueByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllVenuesAsync_ShouldReturnApprovedVenues()
    {
        var venues = new List<Venue>
        {
            new() { VenueId = 1, Name = "Arena 1", OwnerId = 1, Owner = new User { UserId = 1, FullName = "O1" }, Courts = new List<Court>() },
            new() { VenueId = 2, Name = "Arena 2", OwnerId = 2, Owner = new User { UserId = 2, FullName = "O2" }, Courts = new List<Court>() }
        };
        _venueRepoMock.Setup(r => r.GetApprovedVenuesAsync()).ReturnsAsync(venues);

        var result = await _venueService.GetAllVenuesAsync();

        Assert.Equal(2, result.Count());
    }
}

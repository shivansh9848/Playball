using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepoMock.Object);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldUpdateRole()
    {
        var user = new User { UserId = 1, FullName = "Test", Email = "test@e.com", Role = UserRole.User };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.AssignRoleAsync(1, UserRole.VenueOwner);

        Assert.Equal("VenueOwner", result.Role);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.VenueOwner)), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldThrow_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _userService.AssignRoleAsync(99, UserRole.Admin));
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            new() { UserId = 1, FullName = "User 1", Email = "u1@e.com", Role = UserRole.User },
            new() { UserId = 2, FullName = "User 2", Email = "u2@e.com", Role = UserRole.Admin }
        };
        _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldSetInactive()
    {
        var user = new User { UserId = 1, FullName = "Test", Email = "t@e.com", IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.DeactivateUserAsync(1);

        Assert.True(result);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.IsActive == false)), Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldThrow_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeactivateUserAsync(99));
    }

    [Fact]
    public async Task ActivateUserAsync_ShouldSetActive()
    {
        var user = new User { UserId = 1, FullName = "Test", Email = "t@e.com", IsActive = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.ActivateUserAsync(1);

        Assert.True(result);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.IsActive == true)), Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_ShouldThrow_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _userService.ActivateUserAsync(99));
    }
}

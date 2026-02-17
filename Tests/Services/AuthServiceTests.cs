using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace Playball.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRepository<Wallet>> _walletRepoMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _walletRepoMock = new Mock<IRepository<Wallet>>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "SuperSecretKeyForTestingThatIsLongEnough123456!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiryHours", "24" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(_userRepoMock.Object, _walletRepoMock.Object, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsNew()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _walletRepoMock.Setup(r => r.AddAsync(It.IsAny<Wallet>())).ReturnsAsync((Wallet w) => w);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(UserRole.User, result.Role); // Security: always User
        Assert.NotEmpty(result.Token);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _walletRepoMock.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User { UserId = 1, Email = "test@example.com" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(existingUser);

        var request = new RegisterUserRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenPhoneAlreadyExists()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new User { UserId = 1, PhoneNumber = "1234567890" });

        var request = new RegisterUserRequest
        {
            FullName = "Test User",
            Email = "new@example.com",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User
        {
            UserId = 1,
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            PasswordHash = passwordHash,
            Role = UserRole.User,
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("nonexistent@example.com")).ReturnsAsync((User?)null);

        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordWrong()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            IsActive = true
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserInactive()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = false
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Role = UserRole.User,
            IsActive = true,
            AggregatedRating = 4.5m,
            GamesPlayed = 10
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _authService.GetUserProfileAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result!.FullName);
        Assert.Equal(4.5m, result.AggregatedRating);
        Assert.Equal(10, result.GamesPlayed);
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.GetUserProfileAsync(999);

        // Assert
        Assert.Null(result);
    }
}

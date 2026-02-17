using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Common.Helpers;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace Assignment_Example_HU.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(
        IUserRepository userRepository,
        IRepository<Wallet> walletRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;

        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var jwtExpiryHours = int.Parse(configuration["Jwt:ExpiryHours"] ?? "24");

        _jwtHelper = new JwtHelper(jwtSecret, jwtIssuer, jwtAudience, jwtExpiryHours);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BusinessException("User with this email already exists");
        }

        // Check if phone number already exists
        var existingPhone = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingPhone != null)
        {
            throw new BusinessException("User with this phone number already exists");
        }

        // Create new user
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Create wallet for user
        var wallet = new Wallet
        {
            UserId = user.UserId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _walletRepository.AddAsync(wallet);
        await _walletRepository.SaveChangesAsync();

        // Generate token
        var token = _jwtHelper.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        // Verify password
        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Generate token
        var token = _jwtHelper.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            AggregatedRating = user.AggregatedRating,
            GamesPlayed = user.GamesPlayed,
            PreferredSports = user.PreferredSports,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

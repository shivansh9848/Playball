using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;
using Assignment_Example_HU.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace Assignment_Example_HU.Application.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly string _adminSecretKey;

    public AdminAuthService(
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

        // Secret key required to register a new admin (set in appsettings)
        _adminSecretKey = configuration["Admin:SecretKey"]
            ?? "AdminSecret@Playball2026";
    }

    public async Task<AuthResponse> RegisterAdminAsync(RegisterUserRequest request, string adminSecretKey)
    {
        // Validate the admin secret key
        if (adminSecretKey != _adminSecretKey)
            throw new UnauthorizedException("Invalid admin secret key");

        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new BusinessException("User with this email already exists");

        // Check if phone already exists
        var existingPhone = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingPhone != null)
            throw new BusinessException("User with this phone number already exists");

        var admin = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = UserRole.Admin,   // Always Admin for this endpoint

            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(admin);
        await _userRepository.SaveChangesAsync();

        // Create wallet for admin
        var wallet = new Wallet
        {
            UserId = admin.UserId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _walletRepository.AddAsync(wallet);
        await _walletRepository.SaveChangesAsync();

        var token = _jwtHelper.GenerateToken(admin);

        return new AuthResponse
        {
            UserId = admin.UserId,
            FullName = admin.FullName,
            Email = admin.Email,
            PhoneNumber = admin.PhoneNumber,
            Role = admin.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<AuthResponse> LoginAdminAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedException("Invalid email or password");

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");



        // Only allow Admin role users to login via this endpoint
        if (user.Role != UserRole.Admin)
            throw new UnauthorizedException("This endpoint is for admin accounts only");

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

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
}

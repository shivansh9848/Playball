using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse> AssignRoleAsync(int userId, UserRole newRole)
    {
        // GameOwner is auto-assigned when a user creates their first game — Admin cannot assign it manually
        if (newRole == UserRole.GameOwner)
            throw new BusinessException("GameOwner role is automatically assigned when a user creates their first game. It cannot be manually assigned.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

        user.Role = newRole;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

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

    public async Task<IEnumerable<UserProfileResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserProfileResponse
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role.ToString(),
            AggregatedRating = u.AggregatedRating,
            GamesPlayed = u.GamesPlayed,
            PreferredSports = u.PreferredSports,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

        user.IsActive = true;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}

using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> AssignRoleAsync(int userId, UserRole newRole);
    Task<IEnumerable<UserProfileResponse>> GetAllUsersAsync();
    Task<bool> DeactivateUserAsync(int userId);
    Task<bool> ActivateUserAsync(int userId);
}

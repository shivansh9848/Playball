using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileResponse?> GetUserProfileAsync(int userId);
}

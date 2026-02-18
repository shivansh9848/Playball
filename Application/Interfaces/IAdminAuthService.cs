using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IAdminAuthService
{
    Task<AuthResponse> RegisterAdminAsync(RegisterUserRequest request, string adminSecretKey);
    Task<AuthResponse> LoginAdminAsync(LoginRequest request);
}

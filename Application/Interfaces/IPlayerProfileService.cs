using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IPlayerProfileService
{
    Task<PlayerProfileResponse> GetPlayerProfileAsync(int userId);
}

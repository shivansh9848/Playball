using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IWaitlistService
{
    Task<WaitlistResponse> JoinWaitlistAsync(int userId, int gameId);
    Task<IEnumerable<WaitlistResponse>> GetWaitlistAsync(int gameId);
    Task InviteFromWaitlistAsync(int gameOwnerId, int gameId, int waitlistUserId);
    Task RemoveFromWaitlistAsync(int userId, int gameId);
}

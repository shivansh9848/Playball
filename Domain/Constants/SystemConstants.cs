namespace Assignment_Example_HU.Domain.Constants;

public static class SystemConstants
{
    // Waitlist
    public const int DefaultWaitlistCapacity = 10;

    // Slot lock
    public const int SlotLockExpiryMinutes = 5;

    // Game
    public const int MinGameCancellationCheckMinutes = 5;

    // JWT
    public const string JwtIssuer = "PlayballAPI";
    public const string JwtAudience = "PlayballClients";
    public const int JwtExpiryHours = 24;

    // Validation
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 100;
}

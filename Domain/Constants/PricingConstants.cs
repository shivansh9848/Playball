namespace Assignment_Example_HU.Domain.Constants;

public static class PricingConstants
{
    // Demand multipliers
    public const decimal DemandMultiplier_NoViewers = 1.0m;
    public const decimal DemandMultiplier_2To5Viewers = 1.2m;
    public const decimal DemandMultiplier_MoreThan5Viewers = 1.5m;

    // Time-based multipliers
    public const decimal TimeMultiplier_MoreThan24Hours = 1.0m;
    public const decimal TimeMultiplier_6To24Hours = 1.2m;
    public const decimal TimeMultiplier_LessThan6Hours = 1.5m;

    // Historical popularity multipliers
    public const decimal HistoricalMultiplier_Low = 1.0m; // Rating 1-2
    public const decimal HistoricalMultiplier_Medium = 1.2m; // Rating 3
    public const decimal HistoricalMultiplier_High = 1.5m; // Rating 4-5

    // Price lock duration in minutes
    public const int PriceLockDurationMinutes = 5;
}

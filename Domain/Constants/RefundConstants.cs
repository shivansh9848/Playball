namespace Assignment_Example_HU.Domain.Constants;

public static class RefundConstants
{
    // Refund percentages
    public const decimal RefundPercent_MoreThan24Hours = 100m;
    public const decimal RefundPercent_6To24Hours = 50m;
    public const decimal RefundPercent_LessThan6Hours = 0m;
    public const decimal RefundPercent_VenueUnavailable = 100m;

    // Time windows in hours
    public const int FullRefundWindowHours = 24;
    public const int PartialRefundWindowHours = 6;
}

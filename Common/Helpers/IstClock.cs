using System;

namespace Assignment_Example_HU.Common.Helpers;

/// <summary>
/// Provides current time in Indian Standard Time (IST = UTC+5:30).
/// </summary>
public static class IstClock
{
    private static readonly TimeZoneInfo IstZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    /// <summary>Returns the current date and time in IST (UTC+5:30).</summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstZone);

    /// <summary>Returns the current date in IST.</summary>
    public static DateOnly Today => DateOnly.FromDateTime(Now);
}

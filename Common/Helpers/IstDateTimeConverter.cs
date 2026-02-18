using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignment_Example_HU.Common.Helpers;

/// <summary>
/// JSON converter that serializes UTC DateTime values as IST (UTC+5:30)
/// in the format: "2026-02-18T12:07:00+05:30"
/// The database always stores UTC; this only affects the JSON response output.
/// </summary>
public class IstDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo IstZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse incoming datetime and convert to UTC for storage
        var dt = reader.GetDateTime();
        return dt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            : dt.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Convert UTC to IST for display
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        var ist = TimeZoneInfo.ConvertTimeFromUtc(utc, IstZone);
        // Write as ISO 8601 with IST offset
        writer.WriteStringValue(ist.ToString("yyyy-MM-ddTHH:mm:ss+05:30"));
    }
}

/// <summary>
/// Same as IstDateTimeConverter but for nullable DateTime?
/// </summary>
public class IstNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly TimeZoneInfo IstZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var dt = reader.GetDateTime();
        return dt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            : dt.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue) { writer.WriteNullValue(); return; }

        var utc = value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();

        var ist = TimeZoneInfo.ConvertTimeFromUtc(utc, IstZone);
        writer.WriteStringValue(ist.ToString("yyyy-MM-ddTHH:mm:ss+05:30"));
    }
}

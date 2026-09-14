using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Common;

/// <summary>
/// Emits DateTime as ISO-8601 with whole seconds only (no fractional seconds).
/// The Firefall client JSON parser rejects / mishandles sub-second timestamps in character list.
/// </summary>
public sealed class WholeSecondDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (string.IsNullOrEmpty(s))
        {
            return default;
        }

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        var truncated = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, DateTimeKind.Utc);
        writer.WriteStringValue(truncated.ToString(Format, CultureInfo.InvariantCulture));
    }
}

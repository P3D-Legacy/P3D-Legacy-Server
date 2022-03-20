using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    public class DateTimeOffsetNullableSerializer : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetString() is { } raw)
                return DateTimeOffset.ParseExact(raw, "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo));
        }
    }
}
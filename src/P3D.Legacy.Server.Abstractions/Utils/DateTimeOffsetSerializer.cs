using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    public class DateTimeOffsetSerializer : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.ParseExact(reader.GetString() ?? string.Empty, "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo));
        }
    }
}
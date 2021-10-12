using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public class DefaultJsonSerializer
    {
        private static JsonSerializerOptions JsonSerializerOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, CancellationToken ct = default) =>
            JsonSerializer.DeserializeAsync<TValue>(utf8Json, JsonSerializerOptions, ct);
        public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken ct = default) =>
            JsonSerializer.DeserializeAsync(utf8Json, returnType, JsonSerializerOptions, ct);

        public TValue? Deserialize<TValue>(ReadOnlySpan<byte> json) => JsonSerializer.Deserialize<TValue>(json, JsonSerializerOptions);
        public object? Deserialize(ReadOnlySpan<byte> json, Type returnType) => JsonSerializer.Deserialize(json, returnType, JsonSerializerOptions);

#if NET6_0
        public TValue? Deserialize<TValue>(ReadOnlySpan<char> json) => JsonSerializer.Deserialize<TValue>(json, JsonSerializerOptions);
        public object? Deserialize(ReadOnlySpan<char> json, Type returnType) => JsonSerializer.Deserialize(json, returnType, JsonSerializerOptions);
#else
        public TValue? Deserialize<TValue>(string json) => JsonSerializer.Deserialize<TValue>(json, JsonSerializerOptions);
        public object? Deserialize(string json, Type returnType) => JsonSerializer.Deserialize(json, returnType, JsonSerializerOptions);
#endif

        public byte[] SerializeToUtf8Bytes<TValue>(TValue value) => JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);
        public byte[] SerializeToUtf8Bytes(object value, Type inputType) => JsonSerializer.SerializeToUtf8Bytes(value, inputType, JsonSerializerOptions);

        public string Serialize<TValue>(TValue value) => JsonSerializer.Serialize(value, JsonSerializerOptions);
        public string Serialize(object value, Type inputType) => JsonSerializer.Serialize(value, inputType, JsonSerializerOptions);
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace P3D.Legacy.Server.Infrastructure.Utils
{
    // TODO: System.Text.Json https://github.com/dotnet/runtime/issues/38324
    [SuppressMessage("Performance", "CA1812")]
    internal class JsonPathConverter : JsonConverter
    {
        // CanConvert is not called when [JsonConverter] attribute is used
        public override bool CanConvert(Type objectType) => false;

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();

        public override bool CanRead => true;
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var targetObj = FormatterServices.GetUninitializedObject(objectType);

            foreach (var prop in objectType.GetProperties().Where(static p => p.CanRead && p.CanWrite))
            {
                var att = prop.GetCustomAttribute<JsonPropertyAttribute>(inherit: true);

                var jsonPath = att is not null ? att.PropertyName : prop.Name;
                if (string.IsNullOrEmpty(jsonPath)) continue;
                var token = jo.SelectToken(jsonPath);

                if (token is not null && token.Type != JTokenType.Null)
                {
                    var value = token.ToObject(prop.PropertyType, serializer);
                    prop.SetValue(targetObj, value, index: null);
                }
            }

            return targetObj;
        }
    }
}
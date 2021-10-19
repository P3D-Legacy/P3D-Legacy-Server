using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Permissions
{
    public class P3DAPIPermissionQueries : IPermissionQueries
    {
        // TODO: System.Text.Json https://github.com/dotnet/runtime/issues/38324
        private class JsonPathConverter : JsonConverter
        {
            // CanConvert is not called when [JsonConverter] attribute is used
            public override bool CanConvert(Type objectType) => false;

            public override bool CanWrite => false;
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

            public override bool CanRead => true;
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jo = JObject.Load(reader);
                var targetObj = FormatterServices.GetUninitializedObject(objectType);

                foreach (var prop in objectType.GetProperties().Where(p => p.CanRead && p.CanWrite))
                {
                    var att = prop.GetCustomAttribute<JsonPropertyAttribute>(true);

                    var jsonPath = (att is not null ? att.PropertyName : prop.Name);
                    var token = jo.SelectToken(jsonPath);

                    if (token is not null && token.Type != JTokenType.Null)
                    {
                        var value = token.ToObject(prop.PropertyType, serializer);
                        prop.SetValue(targetObj, value, null);
                    }
                }

                return targetObj;
            }
        }

        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IHttpClientFactory _httpClientFactory;

        public P3DAPIPermissionQueries(ILogger<P3DAPIPermissionQueries> logger, TracerProvider traceProvider, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Application");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<PermissionViewModel> GetByGameJoltAsync(ulong id, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("P3DAPIPermissionQueries GetByGameJoltAsync", SpanKind.Client);

            var permissions = PermissionFlags.UnVerified;

            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").GetAsync(
                    $"gamejoltaccount/{id}",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return new PermissionViewModel(permissions);
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    if (JsonConvert.DeserializeObject<GameJoltResponseDTO?>(content) is { User: { }, GameJolt: { } } dto)
                    {
                        permissions &= ~PermissionFlags.UnVerified;
                        permissions |= PermissionFlags.User;

                        foreach (var permissionDto in dto.User.Roles.SelectMany(x => x.Permissions))
                        {
                            if (permissionDto.Name.Equals("gameserver.moderator", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionFlags.Moderator;

                            if (permissionDto.Name.Equals("gameserver.admin", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionFlags.Administrator;

                            if (permissionDto.Name.Equals("gameserver.server", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionFlags.Server;
                        }
                        return new PermissionViewModel(permissions);
                    }
                }
                return new PermissionViewModel(permissions);
            }
            finally
            {
                response.Dispose();
            }
        }

        [JsonConverter(typeof(JsonPathConverter))]
        private record UserResponseDTO(
            [property: JsonProperty("data")] UserDTO User,
            [property: JsonProperty("data.gamejolt")] GameJoltDTO GameJolt,
            [property: JsonProperty("data.forum")] ForumDTO Forum);

        [JsonConverter(typeof(JsonPathConverter))]
        private record GameJoltResponseDTO(
            [property: JsonProperty("data")] GameJoltDTO GameJolt,
            [property: JsonProperty("data.user")] UserDTO User);

        [JsonConverter(typeof(JsonPathConverter))]
        private record ForumResponseDTO(
            [property: JsonProperty("data")] ForumDTO Forum,
            [property: JsonProperty("data.user")] UserDTO User);

        private record UserDTO(
            [property: JsonProperty("id")] int Id,
            [property: JsonProperty("name")] string Name,
            [property: JsonProperty("email")] string Email,
            [property: JsonProperty("username")] string Username,
            [property: JsonProperty("created_at")] DateTime CreatedAt,
            [property: JsonProperty("updated_at")] DateTime UpdatedAt,
            [property: JsonProperty("profile_photo_url")] string ProfilePhotoUrl,
            [property: JsonProperty("roles")] IReadOnlyList<RoleDTO> Roles);

        private record GameJoltDTO(
            [property: JsonProperty("user_id")] int UserId,
            [property: JsonProperty("id")] int Id,
            [property: JsonProperty("username")] string Username,
            [property: JsonProperty("verified_at")] DateTime VerifiedAt,
            [property: JsonProperty("created_at")] DateTime CreatedAt,
            [property: JsonProperty("updated_at")] DateTime UpdatedAt,
            [property: JsonProperty("deleted_at")] DateTime? DeletedAt);

        public record ForumDTO(
            [property: JsonProperty("user_id")] int UserId,
            [property: JsonProperty("username")] string Username,
            [property: JsonProperty("verified_at")] DateTime VerifiedAt,
            [property: JsonProperty("created_at")] DateTime CreatedAt,
            [property: JsonProperty("updated_at")] DateTime UpdatedAt,
            [property: JsonProperty("deleted_at")] DateTime? DeletedAt);

        public record RoleDTO(
            [property: JsonProperty("id")] int Id,
            [property: JsonProperty("name")] string Name,
            [property: JsonProperty("created_at")] DateTime CreatedAt,
            [property: JsonProperty("updated_at")] DateTime UpdatedAt,
            [property: JsonProperty("permissions")] List<PermissionDTO> Permissions);

        public record PermissionDTO(
            [property: JsonProperty("id")] int Id,
            [property: JsonProperty("name")] string Name,
            [property: JsonProperty("created_at")] DateTime CreatedAt,
            [property: JsonProperty("updated_at")] DateTime UpdatedAt);
    }
}
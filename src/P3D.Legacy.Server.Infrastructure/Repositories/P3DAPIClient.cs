using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Models.P3D;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories
{
    internal sealed partial class Pokemon3DAPIClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Tracer _tracer;
        private readonly JsonContextP3D _jsonContext;

        public Pokemon3DAPIClient(IHttpClientFactory httpClientFactory, TracerProvider traceProvider, IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _jsonContext = new JsonContextP3D(new JsonSerializerOptions(jsonSerializerOptions.Get("P3D")));
        }


        public async Task<PermissionEntity> GetByGameJoltIdAsync(GameJoltId id, CancellationToken ct)
        {
            var permissions = PermissionTypes.UnVerified;

            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri($"gamejoltaccount/{id}", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return new PermissionEntity(permissions);
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var responseData = await response.Content.ReadFromJsonAsync(_jsonContext.GameJoltResponseDTO, ct);
                    if (responseData is { UserGameJolt: { User: { } user } })
                    {
                        permissions &= ~PermissionTypes.UnVerified;
                        permissions |= PermissionTypes.User;

                        foreach (var permissionDto in user.Roles?.SelectMany(static x => x.Permissions) ?? Enumerable.Empty<PermissionDTO>())
                        {
                            if (permissionDto.Name.Equals("gameserver.debug", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Debug;

                            if (permissionDto.Name.Equals("gameserver.moderator", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Moderator;

                            if (permissionDto.Name.Equals("gameserver.admin", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Administrator;

                            if (permissionDto.Name.Equals("gameserver.server", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Server;
                        }
                        return new PermissionEntity(permissions);
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    permissions &= ~PermissionTypes.UnVerified;
                    permissions |= PermissionTypes.User;
                }

                return new PermissionEntity(permissions);
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<BanResponseEntry?> GetBanAsync(PlayerId id, CancellationToken ct)
        {
            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri($"ban/gamejoltaccount/{id.GameJoltIdOrNone}", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return null;
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    var responseData = JsonSerializer.Deserialize(content, _jsonContext.BanListResponse);
                    if (responseData is null || responseData.Entries is null)
                        return null;

                    var permaban = responseData.Entries.FirstOrDefault(static x => x is not null && (x.ExpireAt is null));
                    var maxExpire = responseData.Entries.Where(static x => x is not null && x.ExpireAt is not null).MaxBy(static x => x!.ExpireAt);
                    return permaban ?? maxExpire;
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var (bannerId, id, ip, reasonId, reason, expiration) = banEntity;

            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = string.IsNullOrEmpty(reason)
                    ? await client.PostAsJsonAsync(
                        new Uri("ban/gamejoltaccount", UriKind.Relative),
                        new BanRequest(id.GameJoltIdOrNone, reasonId, expiration, bannerId.GameJoltIdOrNone), _jsonContext.BanRequest, ct)
                    : await client.PostAsJsonAsync(
                        new Uri("ban/gamejoltaccount", UriKind.Relative),
                        new TextReasonBanRequest(id.GameJoltIdOrNone, reason, expiration, bannerId.GameJoltIdOrNone), _jsonContext.TextReasonBanRequest, ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return false;
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    return true;
                }
                return false;
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct)
        {
            while (true)
            {
                var currentBan = await GetBanAsync(id, ct);
                if (currentBan is null)
                    return true;


                HttpResponseMessage response;

                try
                {
                    using var client = _httpClientFactory.CreateClient("P3D.API");
                    response = await client.DeleteAsync(
                        new Uri($"ban/gamejoltaccount/{currentBan.Uid}", UriKind.Relative),
                        ct);
                }
                catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
                {
                    return false;
                }

                try
                {
                    if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        return true;
                    }
                    return false;
                }
                finally
                {
                    response.Dispose();
                }
            }
        }

        public async Task<UserDTO?> GetUserAsync(ulong userId, CancellationToken ct)
        {
            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri($"user/{userId}", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return null;
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var responseData = await response.Content.ReadFromJsonAsync(_jsonContext.UserResponse, ct);
                    if (responseData is null || responseData.User is null)
                        return null;

                    return responseData.User;
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<ReasonDTO?> GetReasonAsync(ulong reasonId, CancellationToken ct)
        {
            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri($"banreason/{reasonId}", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return null;
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var responseData = await response.Content.ReadFromJsonAsync(_jsonContext.ReasonResponse, ct);
                    if (responseData is null || responseData.Entries is null)
                        return null;

                    return responseData.Entries.FirstOrDefault();
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }


        [JsonSerializable(typeof(GameJoltResponseDTO))]
        [JsonSerializable(typeof(BanRequest))]
        [JsonSerializable(typeof(TextReasonBanRequest))]
        [JsonSerializable(typeof(BanListResponse))]
        [JsonSerializable(typeof(UserResponse))]
        [JsonSerializable(typeof(ReasonResponse))]
        [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
        internal partial class JsonContextP3D : JsonSerializerContext { }

        internal sealed record GameJoltResponseDTO(
            [property: JsonPropertyName("data")] GameJoltDTO UserGameJolt);

        internal sealed record UserResponse(
            [property: JsonPropertyName("data")] UserDTO User);

        internal sealed record ReasonResponse(
            [property: JsonPropertyName("data")] IReadOnlyList<ReasonDTO?>? Entries);

        internal sealed record BanListResponse(
            [property: JsonPropertyName("data")] IReadOnlyList<BanResponseEntry?>? Entries);

        internal sealed record BanResponseEntry(
            [property: JsonPropertyName("uuid")] Guid? Uid,
            [property: JsonPropertyName("banned_by")] UserDTO? BannedBy,
            [property: JsonPropertyName("reason")] ReasonDTO? Reason,
            [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpireAt);

        internal sealed record BanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason_id")] ulong ReasonId,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );

        internal sealed record TextReasonBanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason")] string Reason,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );
    }
}
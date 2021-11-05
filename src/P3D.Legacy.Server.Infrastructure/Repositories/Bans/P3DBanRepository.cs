using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Models.P3D;
using P3D.Legacy.Server.Infrastructure.Utils;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans
{
    public class P3DBanRepository
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DefaultJsonSerializer _defaultJsonSerializer;

        public P3DBanRepository(ILogger<P3DBanRepository> logger, TracerProvider traceProvider, IHttpClientFactory httpClientFactory, DefaultJsonSerializer defaultJsonSerializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _defaultJsonSerializer = defaultJsonSerializer ?? throw new ArgumentNullException(nameof(defaultJsonSerializer));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository UnbanAsync", SpanKind.Client);

            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").GetAsync(
                    $"ban/gamejoltaccount/{id.Id}",
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
                    var responseData = JsonConvert.DeserializeObject<BanUserResponse?>(content);
                    if (responseData is null || responseData.Entry is null || responseData.Entry.UserGameJolt is null || responseData.Entry.User is null)
                        return null;

                    return new BanEntity(
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(responseData.Entry.Banner.Id)),
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(responseData.Entry.UserGameJolt.Id)),
                            IPAddress.None,
                            0,
                            responseData.Entry.Reason,
                            responseData.Entry.ExpireAt);
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        public async IAsyncEnumerable<BanEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository UnbanAsync", SpanKind.Client);

            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").GetAsync(
                    $"ban/gamejoltaccount",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                yield break;
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    var responseData = JsonConvert.DeserializeObject<BanListResponse?>(content);
                    if (responseData is null || responseData.Entries is null)
                        yield break;

                    foreach (var data in responseData.Entries)
                    {
                        if (data is null || data.UserGameJolt is null || data.User is null)
                            continue;

                        yield return new BanEntity(
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.Banner.Id)),
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.UserGameJolt.Id)),
                            IPAddress.None,
                            0,
                            data.Reason,
                            data.ExpireAt);
                    }
                }
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository BanAsync", SpanKind.Client);

            var (bannerId, id, ip, reasonId, reason, expiration) = banEntity;

            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").PostAsJsonAsync(
                    $"ban/gamejoltaccount/{id.Id}",
                    ulong.TryParse(reason, out var reasonInt)
                        ? (object) new BanRequest(GameJoltId.Parse(id.Id), reasonInt, expiration, GameJoltId.Parse(bannerId.Id))
                        : (object) new TextReasonBanRequest(GameJoltId.Parse(id.Id), reason, expiration, GameJoltId.Parse(bannerId.Id)),
                    _defaultJsonSerializer.Options,
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

        public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository UnbanAsync", SpanKind.Client);

            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").DeleteAsync(
                    $"ban/gamejoltaccount/{id.Id}",
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

        [Newtonsoft.Json.JsonConverter(typeof(JsonPathConverter))]
        private record BanUserResponse(
            [property: JsonProperty("data[0]")] BanResponseEntry Entry);

        private record BanListResponse(
            [property: JsonProperty("data")] IReadOnlyList<BanResponseEntry> Entries);

        [Newtonsoft.Json.JsonConverter(typeof(JsonPathConverter))]
        private record BanResponseEntry(
            [property: JsonProperty("gamejoltaccount")] GameJoltDTO UserGameJolt,
            [property: JsonProperty("gamejoltaccount.user")] UserDTO User,
            [property: JsonProperty("banned_by")] UserDTO Banner,
            [property: JsonProperty("reason.name")] string Reason,
            [property: JsonProperty("updated_at")] DateTimeOffset UpdatedAt,
            [property: JsonProperty("expire_at")] DateTimeOffset? ExpireAt);



        private record BanRequestData(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason_id")] ulong ReasonId,
            [property: JsonPropertyName("expires_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );
        private sealed record BanRequest
        {
            [JsonPropertyName("data")] public BanRequestData Data { get; init; }

            public BanRequest(ulong gameJoltId, ulong reasonId, DateTimeOffset? expiresAt, ulong bannedByGameJoltId)
            {
                Data = new BanRequestData(gameJoltId, reasonId, expiresAt, bannedByGameJoltId);
            }
        }

        private record BanResponseData(
            [property: JsonPropertyName("expires_at")] DateTimeOffset? ExpiresAt);
        private sealed record BanResponse(
            [property: JsonPropertyName("data")] BanResponseData[] Data);

        private sealed record TextReasonBanRequest
        {
            public record BanRequestData(
                [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
                [property: JsonPropertyName("reason")] string Reason,
                [property: JsonPropertyName("expires_at")] DateTimeOffset? ExpiresAt,
                [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
            );

            [JsonPropertyName("data")] public BanRequestData Data { get; init; }

            public TextReasonBanRequest(ulong gameJoltId, string reason, DateTimeOffset? expiresAt, ulong bannedByGameJoltId)
            {
                Data = new BanRequestData(gameJoltId, reason, expiresAt, bannedByGameJoltId);
            }
        };
    }
}
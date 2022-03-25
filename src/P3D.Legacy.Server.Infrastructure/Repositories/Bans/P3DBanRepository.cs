using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Models.P3D;
using P3D.Legacy.Server.Infrastructure.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans
{
    public class P3DBanRepository
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public P3DBanRepository(ILogger<P3DBanRepository> logger, TracerProvider traceProvider, HttpClient httpClient, IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonSerializerOptions = jsonSerializerOptions.Get("P3D") ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository GetAsync", SpanKind.Client);

            var actualEntry = await GetInternal(id, ct);
            return actualEntry is null
                ? null
                : new BanEntity(
                    PlayerId.FromGameJolt(GameJoltId.FromNumber(actualEntry.Banner.Id)),
                    PlayerId.FromGameJolt(GameJoltId.FromNumber(actualEntry.UserGameJolt.Id)),
                    IPAddress.None,
                    0,
                    actualEntry.Reason,
                    actualEntry.ExpireAt);
        }

        public async IAsyncEnumerable<BanEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository GetAllAsync", SpanKind.Client);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(
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
                response = await _httpClient.PostAsJsonAsync(
                    $"ban/gamejoltaccount",
                    string.IsNullOrEmpty(reason)
                        ? (object) new BanRequest(GameJoltId.Parse(id.Id), reasonId, expiration, GameJoltId.Parse(bannerId.Id))
                        : (object) new TextReasonBanRequest(GameJoltId.Parse(id.Id), reason, expiration, GameJoltId.Parse(bannerId.Id)),
                    _jsonSerializerOptions,
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

            return await UnbanInternal(id, ct);
        }

        private async Task<BanResponseEntry?> GetInternal(PlayerId id, CancellationToken ct)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(
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
                    var responseData = JsonConvert.DeserializeObject<BanListResponse?>(content);
                    if (responseData is null || responseData.Entries is null)
                        return null;

                    return responseData.Entries.FirstOrDefault(x => x.ExpireAt is null || x.ExpireAt > DateTimeOffset.UtcNow);
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        private async Task<bool> UnbanInternal(PlayerId id, CancellationToken ct)
        {
            while (true)
            {
                var currentBan = await GetInternal(id, ct);
                if (currentBan is null)
                    return true;


                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.DeleteAsync(
                        $"ban/gamejoltaccount/{currentBan.Uid}",
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

        private record BanListResponse(
            [property: JsonProperty("data")] IReadOnlyList<BanResponseEntry> Entries);

        [Newtonsoft.Json.JsonConverter(typeof(JsonPathConverter))]
        private record BanResponseEntry(
            [property: JsonProperty("uuid")] Guid Uid,
            [property: JsonProperty("gamejoltaccount")] GameJoltDTO UserGameJolt,
            [property: JsonProperty("gamejoltaccount.user")] UserDTO User,
            [property: JsonProperty("banned_by")] UserDTO Banner,
            [property: JsonProperty("reason.name")] string Reason,
            [property: JsonProperty("updated_at")] DateTimeOffset UpdatedAt,
            [property: JsonProperty("expire_at")] DateTimeOffset? ExpireAt);


        private sealed record BanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason_id")] ulong ReasonId,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );

        private sealed record TextReasonBanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason")] string Reason,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );
    }
}
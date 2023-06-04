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
using System.Diagnostics.CodeAnalysis;
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
    [RequiresUnreferencedCode("Newtonsoft.Json")]
    public partial class P3DBanRepository
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonContext _jsonContext;

        public P3DBanRepository(ILogger<P3DBanRepository> logger, TracerProvider traceProvider, IHttpClientFactory httpClientFactory, IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jsonContext = new JsonContext(new JsonSerializerOptions(jsonSerializerOptions.Get("P3D")));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository GetAsync", SpanKind.Client);

            var actualEntry = await GetInternalAsync(id, ct);
            return actualEntry is null || actualEntry.Banner is null || actualEntry.UserGameJolt is null || actualEntry.Reason is null
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
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri("ban/gamejoltaccount", UriKind.Relative),
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
                    if (responseData?.Entries is null)
                        yield break;

                    foreach (var data in responseData.Entries)
                    {
                        if (data is null || data.Banner is null || data.UserGameJolt is null || data.Reason is null)
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
            ct.ThrowIfCancellationRequested();

            using var span = _tracer.StartActiveSpan("P3DBanRepository UnbanAsync", SpanKind.Client);

            return await UnbanInternalAsync(id, ct);
        }

        private async Task<BanResponseEntry?> GetInternalAsync(PlayerId id, CancellationToken ct)
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
                    var responseData = JsonConvert.DeserializeObject<BanListResponse?>(content);
                    if (responseData is null || responseData.Entries is null)
                        return null;

                    return responseData.Entries.FirstOrDefault(static x => x is not null && (x.ExpireAt is null || x.ExpireAt > DateTimeOffset.UtcNow));
                }
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        private async Task<bool> UnbanInternalAsync(PlayerId id, CancellationToken ct)
        {
            while (true)
            {
                var currentBan = await GetInternalAsync(id, ct);
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

        [SuppressMessage("Performance", "CA1812")]
        private record BanListResponse(
            [property: JsonProperty("data")] IReadOnlyList<BanResponseEntry?>? Entries);

        [Newtonsoft.Json.JsonConverter(typeof(JsonPathConverter))]
        [SuppressMessage("Performance", "CA1812")]
        private record BanResponseEntry(
            [property: JsonProperty("uuid")] Guid? Uid,
            [property: JsonProperty("gamejoltaccount")] GameJoltDTO? UserGameJolt,
            [property: JsonProperty("gamejoltaccount.user")] UserDTO? User,
            [property: JsonProperty("banned_by")] UserDTO? Banner,
            [property: JsonProperty("reason.name")] string? Reason,
            [property: JsonProperty("updated_at")] DateTimeOffset? UpdatedAt,
            [property: JsonProperty("expire_at")] DateTimeOffset? ExpireAt);


        [JsonSerializable(typeof(BanRequest))]
        [JsonSerializable(typeof(TextReasonBanRequest))]
        internal partial class JsonContext : JsonSerializerContext { }

#pragma warning disable SYSLIB1037
        internal sealed record BanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason_id")] ulong ReasonId,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );
#pragma warning restore SYSLIB1037

#pragma warning disable SYSLIB1037
        internal sealed record TextReasonBanRequest(
            [property: JsonPropertyName("gamejoltaccount_id")] ulong GameJoltId,
            [property: JsonPropertyName("reason")] string Reason,
            [property: JsonPropertyName("expire_at")] DateTimeOffset? ExpiresAt,
            [property: JsonPropertyName("banned_by_gamejoltaccount_id")] ulong BannedByGameJoltId
        );
#pragma warning restore SYSLIB1037
    }
}
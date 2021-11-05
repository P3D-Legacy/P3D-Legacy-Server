using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Collections.Generic;
using System.Linq;
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
                    var data = await response.Content.ReadFromJsonAsync<BanRequestData?>(cancellationToken: ct);
                    return data is null
                        ? null
                        : new BanEntity(
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.BannedByGameJoltId)),
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.GameJoltId)),
                            IPAddress.None,
                            data.ReasonId.ToString(),
                            data.ExpiresAt);
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
                    var datas = await response.Content.ReadFromJsonAsync<BanRequestData[]?>(cancellationToken: ct);
                    foreach (var data in datas ?? Enumerable.Empty<BanRequestData>())
                    {
                        yield return new BanEntity(
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.BannedByGameJoltId)),
                            PlayerId.FromGameJolt(GameJoltId.FromNumber(data.GameJoltId)),
                            IPAddress.None,
                            data.ReasonId.ToString(),
                            data.ExpiresAt);
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

            var (bannerId, id, ip, reason, expiration) = banEntity;

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
        };

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
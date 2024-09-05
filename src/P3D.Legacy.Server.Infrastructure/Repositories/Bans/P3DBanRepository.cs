using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans;

// TODO: public
internal class P3DBanRepository
{
    private readonly ILogger _logger;
    private readonly Tracer _tracer;
    private readonly Pokemon3DAPIClient _apiClient;

    public P3DBanRepository(ILogger<P3DBanRepository> logger, TracerProvider traceProvider, Pokemon3DAPIClient apiClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var span = _tracer.StartActiveSpan("P3DBanRepository GetAsync", SpanKind.Client);

        var actualEntry = await _apiClient.GetBanAsync(id, ct);
        if (actualEntry is null || actualEntry.BannedBy is null || actualEntry.Reason is null) return null;

        return new BanEntity(
            PlayerId.FromGameJolt(GameJoltId.From(actualEntry.BannedBy.Id)),
            id,
            IPAddress.None,
            0,
            actualEntry.Reason.Name ?? "Unknown reason",
            actualEntry.ExpireAt);
    }

    public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var span = _tracer.StartActiveSpan("P3DBanRepository BanAsync", SpanKind.Client);

        return await _apiClient.BanAsync(banEntity, ct);
    }

    public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var span = _tracer.StartActiveSpan("P3DBanRepository UnbanAsync", SpanKind.Client);

        return await _apiClient.UnbanAsync(id, ct);
    }
}
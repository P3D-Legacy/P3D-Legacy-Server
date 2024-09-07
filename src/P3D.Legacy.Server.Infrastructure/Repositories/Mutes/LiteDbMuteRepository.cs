using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Domain.Repositories;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Mutes;

public class LiteDbMuteRepository : IMuteRepository
{
    private record Mute(string Id, IList<string> MutedIds);

    private readonly LiteDbOptions _options;
    private readonly Tracer _tracer;

    public LiteDbMuteRepository(IOptionsMonitor<LiteDbOptions> options, TracerProvider traceProvider)
    {
        _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
    }

    public async IAsyncEnumerable<PlayerId> GetAllAsync(PlayerId id, [EnumeratorCancellation] CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Get All Mutes", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);
        span.SetAttribute("peer.service", "LiteDB");

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Mute>("mutes");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        ct.ThrowIfCancellationRequested();
        if (await collection.FindByIdAsync(idStr) is { } entry)
        {
            foreach (var playerId in entry.MutedIds.Select(static x => PlayerId.Parse(x)))
            {
                yield return playerId;
            }
        }
    }

    public async Task<bool> IsMutedAsync(PlayerId id, PlayerId toCheckId, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Is Muted", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);
        span.SetAttribute("peer.service", "LiteDB");

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Mute>("mutes");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        var toCheckIdStr = toCheckId.ToString();
        ct.ThrowIfCancellationRequested();
        return await collection.ExistsAsync(Query.And(Query.EQ(nameof(Mute.Id), idStr), Query.In(nameof(Mute.MutedIds), toCheckIdStr)));
    }

    public async Task<bool> MuteAsync(PlayerId id, PlayerId toMuteId, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Mute", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);
        span.SetAttribute("peer.service", "LiteDB");

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Mute>("mutes");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        ct.ThrowIfCancellationRequested();
        if (await collection.FindByIdAsync(idStr) is { } entry)
        {
            ct.ThrowIfCancellationRequested();
            entry.MutedIds.Add(toMuteId.ToString());
            return await collection.UpdateAsync(entry);
        }
        else
        {
            ct.ThrowIfCancellationRequested();
            return await collection.InsertAsync(new Mute(id.ToString(), new List<string> { toMuteId.ToString() })) is not null;
        }
    }

    public async Task<bool> UnmuteAsync(PlayerId id, PlayerId toUnmuteId, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Unmute", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);
        span.SetAttribute("peer.service", "LiteDB");

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Mute>("mutes");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        ct.ThrowIfCancellationRequested();
        if (await collection.FindByIdAsync(idStr) is { } entry)
        {
            ct.ThrowIfCancellationRequested();
            return entry.MutedIds.Remove(toUnmuteId.ToString()) && await collection.UpdateAsync(entry);
        }

        return false;
    }
}
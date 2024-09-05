using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans;

public class LiteDbBanRepository
{
    private record Ban(string Id, string BannerId, string Ip, ulong ReasonId, string Reason, DateTimeOffset? Expiration)
    {
        public Ban() : this(default!, default!, default!, default, default!, default) { }
    }

    private readonly LiteDbOptions _options;
    private readonly Tracer _tracer;

    public LiteDbBanRepository(IOptionsMonitor<LiteDbOptions> options, TracerProvider traceProvider)
    {
        _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
    }

    public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Get Ban", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);
        span.SetAttribute("peer.service", "LiteDB");

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Ban>("bans");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        ct.ThrowIfCancellationRequested();
        var entry = await collection.FindByIdAsync(idStr) is { } ban ? new BanEntity(PlayerId.Parse(ban.BannerId), PlayerId.Parse(ban.Id), IPAddress.Parse(ban.Ip), ban.ReasonId, ban.Reason, ban.Expiration) : null;

        return entry;
    }

    public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Ban", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);

        ct.ThrowIfCancellationRequested();

        var (bannerId, id, ip, reasonId, reason, expiration) = banEntity;

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Ban>("bans");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        var bannerIdStr = bannerId.ToString();
        ct.ThrowIfCancellationRequested();
        var entry = await collection.UpsertAsync(new Ban(idStr, bannerIdStr, ip.ToString(), reasonId, reason, expiration));

        return entry;
    }

    public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct)
    {
        var connectionString = new ConnectionString(_options.ConnectionString);

        using var span = _tracer.StartActiveSpan("Unban", SpanKind.Client);
        span.SetAttribute("client.address", connectionString.Filename);

        ct.ThrowIfCancellationRequested();

        using var db = new LiteDatabaseAsync(connectionString);
        var collection = db.GetCollection<Ban>("bans");

        ct.ThrowIfCancellationRequested();
        await collection.EnsureIndexAsync(static x => x.Id, true);

        var idStr = id.ToString();
        ct.ThrowIfCancellationRequested();
        var count = await collection.DeleteManyAsync(Query.EQ(nameof(Ban.Id), idStr));

        return count > 0;
    }
}
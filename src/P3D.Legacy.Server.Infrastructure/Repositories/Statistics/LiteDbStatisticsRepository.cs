using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Statistics;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Statistics
{
    public class LiteDbStatisticsRepository
    {
        private record Statistics(string Id, string PlayerId, string Action, int Count)
        {
            public Statistics() : this(default!, default!, default!, default) { }
        }

        private readonly LiteDbOptions _options;
        private readonly Tracer _tracer;

        public LiteDbStatisticsRepository(IOptionsMonitor<LiteDbOptions> options, TracerProvider traceProvider)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
        }

        public async Task<StatisticsEntity?> GetAsync(PlayerId id, string action, CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get Statistics", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Statistics>("statistics");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);
            await collection.EnsureIndexAsync(static x => x.PlayerId);
            await collection.EnsureIndexAsync(static x => x.Action);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync($"{idStr}|{action}") is { } statistics ? new StatisticsEntity(PlayerId.Parse(statistics.PlayerId), statistics.Action, statistics.Count) : null;

            return entry;
        }

        public async IAsyncEnumerable<StatisticsEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get All Statistics", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Statistics>("statistics");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);
            await collection.EnsureIndexAsync(static x => x.PlayerId);
            await collection.EnsureIndexAsync(static x => x.Action);

            ct.ThrowIfCancellationRequested();
            var entries = await collection.FindAllAsync();
            foreach (var statisticsEntity in entries.Select(static x => new StatisticsEntity(PlayerId.Parse(x.PlayerId), x.Action, x.Count)))
            {
                yield return statisticsEntity;
            }
        }

        public async IAsyncEnumerable<StatisticsEntity> GetAllAsync(PlayerId id, [EnumeratorCancellation] CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get All Statistics By Id", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Statistics>("statistics");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);
            await collection.EnsureIndexAsync(static x => x.PlayerId);
            await collection.EnsureIndexAsync(static x => x.Action);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entries = await collection.FindAsync(x => x.PlayerId == idStr);
            foreach (var statisticsEntity in entries.Select(static x => new StatisticsEntity(PlayerId.Parse(x.PlayerId), x.Action, x.Count)))
            {
                yield return statisticsEntity;
            }
        }

        public async IAsyncEnumerable<StatisticsEntity> GetAllAsync(string action, [EnumeratorCancellation] CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get All Statistics By Action", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Statistics>("statistics");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);
            await collection.EnsureIndexAsync(static x => x.PlayerId);
            await collection.EnsureIndexAsync(static x => x.Action);

            ct.ThrowIfCancellationRequested();
            var entries = await collection.FindAsync(x => x.Action == action);
            foreach (var statisticsEntity in entries.Select(static x => new StatisticsEntity(PlayerId.Parse(x.PlayerId), x.Action, x.Count)))
            {
                yield return statisticsEntity;
            }
        }

        public async Task<bool> IncrementActionAsync(PlayerId id, string action, CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Increment Statistics", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Statistics>("statistics");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);
            await collection.EnsureIndexAsync(static x => x.PlayerId);
            await collection.EnsureIndexAsync(static x => x.Action);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            using (var transaction = await db.BeginTransactionAsync())
            {
                if (await collection.FindByIdAsync($"{idStr}|{action}") is null)
                    await collection.UpsertAsync($"{idStr}|{action}", new Statistics { PlayerId = idStr, Action = action, Count = 0 });
                await transaction.CommitAsync();
            }

            ct.ThrowIfCancellationRequested();
            var result = await collection.UpdateManyAsync(static x => new Statistics { Count = x.Count + 1 }, x => x.PlayerId == idStr && x.Action == action);
            return result == 1;
        }
    }
}
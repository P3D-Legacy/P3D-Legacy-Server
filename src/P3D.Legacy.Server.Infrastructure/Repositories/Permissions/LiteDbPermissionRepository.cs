using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Permissions
{
    public class LiteDbPermissionRepository
    {
        private record Permission(string Id, PermissionTypes Permissions)
        {
            public Permission() : this(default!, default) { }
        }

        private readonly LiteDbOptions _options;
        private readonly Tracer _tracer;

        public LiteDbPermissionRepository(IOptionsMonitor<LiteDbOptions> options, TracerProvider traceProvider)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
        }

        public async Task<PermissionEntity> GetByNameIdAsync(string name, CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get Permission By Id", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);

            var idStr = PlayerId.FromName(name).ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr);

            return new PermissionEntity(entry?.Permissions ?? PermissionTypes.User);
        }

        public async Task<PermissionEntity> GetByGameJoltIdAsync(GameJoltId gameJoltId, CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Get Permission By GameJolt", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);

            var idStr = PlayerId.FromGameJolt(gameJoltId).ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr);

            return new PermissionEntity(entry?.Permissions ?? PermissionTypes.User);
        }

        public async Task<bool> UpdateAsync(PlayerId id, PermissionTypes permissions, CancellationToken ct)
        {
            var connectionString = new ConnectionString(_options.ConnectionString);

            using var span = _tracer.StartActiveSpan("Update Permission", SpanKind.Client);
            span.SetAttribute("client.address", connectionString.Filename);
            span.SetAttribute("peer.service", "LiteDB");

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(connectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            return await collection.ExistsAsync(Query.EQ(nameof(Permission.Id), idStr)) || await collection.UpsertAsync(new Permission(idStr, permissions));
        }
    }
}
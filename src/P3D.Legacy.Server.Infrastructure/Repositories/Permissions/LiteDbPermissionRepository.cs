using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

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
        private record Permission(string Id, PermissionFlags Permissions);

        private readonly LiteDbOptions _options;

        public LiteDbPermissionRepository(IOptions<LiteDbOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<PermissionEntity> GetByNameIdAsync(string name, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            var idStr = PlayerId.FromName(name).ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr);

            return new PermissionEntity(entry?.Permissions ?? PermissionFlags.User);
        }

        public async Task<PermissionEntity> GetByGameJoltIdAsync(GameJoltId gameJoltId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            var idStr = PlayerId.FromGameJolt(gameJoltId).ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr);

            return new PermissionEntity(entry?.Permissions ?? PermissionFlags.User);
        }

        public async Task<bool> UpdateAsync(PlayerId id, PermissionFlags permissions, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Permission>("permissions");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            return await collection.ExistsAsync(Query.EQ(nameof(Permission.Id), idStr)) || await collection.UpsertAsync(new Permission(idStr, permissions));
        }
    }
}
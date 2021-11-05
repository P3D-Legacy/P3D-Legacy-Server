using LiteDB.Async;

using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Bans
{
    public class LiteDbBanRepository
    {
        private record Ban(string Id, string BannerId, string Ip, ulong ReasonId, string Reason, DateTimeOffset? Expiration);

        private readonly LiteDbOptions _options;

        public LiteDbBanRepository(IOptions<LiteDbOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindOneAsync(x => x.Id == idStr) is { } ban ? new BanEntity(PlayerId.Parse(ban.BannerId), PlayerId.Parse(ban.Id), IPAddress.Parse(ban.Ip), ban.ReasonId, ban.Reason, ban.Expiration) : null;

            return entry;
        }

        public async IAsyncEnumerable<BanEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id);

            ct.ThrowIfCancellationRequested();
            var entries = await collection.FindAsync(x => true);
            foreach (var banEntity in entries.Select(ban => new BanEntity(PlayerId.Parse(ban.BannerId), PlayerId.Parse(ban.Id), IPAddress.Parse(ban.Ip), ban.ReasonId, ban.Reason, ban.Expiration)))
            {
                yield return banEntity;
            }
        }

        public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var (bannerId, id, ip, reasonId, reason, expiration) = banEntity;

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id);

            var idStr = id.ToString();
            var bannerIdStr = bannerId.ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.UpsertAsync(new Ban(idStr, bannerIdStr, ip.ToString(), reasonId, reason, expiration));

            return entry;
        }

        public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var count = await collection.DeleteManyAsync(x => x.Id == idStr);

            return count > 0;
        }
    }
}
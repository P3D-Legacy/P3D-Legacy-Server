using LiteDB;
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
        private record Ban(string Id, string BannerId, string Ip, ulong ReasonId, string Reason, DateTimeOffset? Expiration)
        {
            public Ban() : this(default!, default!, default!, default, default!, default) { }
        }

        private readonly LiteDbOptions _options;

        public LiteDbBanRepository(IOptionsMonitor<LiteDbOptions> options)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr) is { } ban ? new BanEntity(PlayerId.Parse(ban.BannerId), PlayerId.Parse(ban.Id), IPAddress.Parse(ban.Ip), ban.ReasonId, ban.Reason, ban.Expiration) : null;

            return entry;
        }

        public async IAsyncEnumerable<BanEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Ban>("bans");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(static x => x.Id, true);

            ct.ThrowIfCancellationRequested();
            var entries = await collection.FindAllAsync();
            foreach (var banEntity in entries.Select(static x => new BanEntity(PlayerId.Parse(x.BannerId), PlayerId.Parse(x.Id), IPAddress.Parse(x.Ip), x.ReasonId, x.Reason, x.Expiration)))
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
            await collection.EnsureIndexAsync(static x => x.Id, true);

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
            await collection.EnsureIndexAsync(static x => x.Id, true);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var count = await collection.DeleteManyAsync(Query.EQ(nameof(Ban.Id), idStr));

            return count > 0;
        }
    }
}
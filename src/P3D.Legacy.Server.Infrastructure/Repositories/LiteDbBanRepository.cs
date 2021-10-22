using LiteDB;

using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories
{
    public class LiteDbBanRepository : IBanRepository
    {
        private record Ban(ulong GameJoltId, string Name, string Ip, string Reason, DateTimeOffset? Expiration);

        private readonly LiteDbOptions _options;

        public LiteDbBanRepository(IOptions<LiteDbOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<bool> UpsertAsync(BanEntity banEntity, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var (id, name, ip, reason, expiration) = banEntity;

            using var db = new LiteDatabase(_options.Path);
            var bans = db.GetCollection<Ban>("bans");

            var result = bans.Upsert(new Ban(id, name, ip.ToString(), reason, expiration));
            return Task.FromResult(result);
        }

        public Task<bool> DeleteAsync(GameJoltId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabase(_options.Path);
            var bans = db.GetCollection<Ban>("bans");

            var count = bans.DeleteMany(x => x.GameJoltId == id);
            return Task.FromResult(count > 0);
        }

        public Task<BanEntity?> GetAsync(GameJoltId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabase(_options.Path);
            var bans = db.GetCollection<Ban>("bans");
            var result = bans.FindOne(x => x.GameJoltId == (ulong) id) is { } ban ? new BanEntity(ban.GameJoltId, ban.Name, IPAddress.Parse(ban.Ip), ban.Reason, ban.Expiration) : null;
            return Task.FromResult(result);
        }

        public IAsyncEnumerable<BanEntity> GetAllAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabase(_options.Path);
            var bans = db.GetCollection<Ban>("bans");
            var result = bans.Find(x => true).Select(ban => new BanEntity(ban.GameJoltId, ban.Name, IPAddress.Parse(ban.Ip), ban.Reason, ban.Expiration));
            return result.ToAsyncEnumerable();
        }
    }
}
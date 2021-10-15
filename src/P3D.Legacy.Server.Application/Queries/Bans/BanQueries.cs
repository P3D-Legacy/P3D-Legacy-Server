using LiteDB;

using P3D.Legacy.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Bans
{
    public sealed class DefaultBanQueries : IBanQueries, IDisposable
    {
        private record Ban(ulong GameJoltId, string Name, string IP, string Reason, DateTimeOffset? Expiration);

        private readonly LiteDatabase _database;

        public DefaultBanQueries()
        {
            _database = new LiteDatabase("bans.litedb");
        }

        public async Task<BanViewModel?> GetAsync(GameJoltId id, CancellationToken ct)
        {
            var bans = _database.GetCollection<Ban>("bans");
            return bans.FindOne(x => x.GameJoltId == (ulong) id) is { } ban ? new BanViewModel(ban.GameJoltId, ban.Name, IPAddress.Parse(ban.IP), ban.Reason, ban.Expiration) : null;
        }

        public IAsyncEnumerable<BanViewModel> GetAllAsync(CancellationToken ct)
        {
            var bans = _database.GetCollection<Ban>("bans");
            return bans.Find(x => true).ToAsyncEnumerable().Select(ban => new BanViewModel(ban.GameJoltId, ban.Name, IPAddress.Parse(ban.IP), ban.Reason, ban.Expiration));
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
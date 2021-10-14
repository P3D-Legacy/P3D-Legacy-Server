using P3D.Legacy.Common;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories
{
    public class BanRepository
    {
        public Task<bool> DeleteAsync(GameJoltId id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Upsert(GameJoltId id, string name, IPAddress Ip, string reason, DateTimeOffset? expiration, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

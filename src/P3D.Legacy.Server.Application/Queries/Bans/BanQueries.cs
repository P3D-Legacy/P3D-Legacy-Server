using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Bans
{
    public sealed class BanQueries : IBanQueries
    {
        private readonly IBanRepository _banRepository;

        public BanQueries(IBanRepository banRepository)
        {
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<BanViewModel?> GetAsync(GameJoltId id, CancellationToken ct)
        {
            return await _banRepository.GetAsync(id, ct) is { } ban ? new BanViewModel(ban.Id, ban.Name, ban.Ip, ban.Reason, ban.Expiration) : null;
        }

        public IAsyncEnumerable<BanViewModel> GetAllAsync(CancellationToken ct)
        {
            return _banRepository.GetAllAsync(ct).Select(ban => new BanViewModel(ban.Id, ban.Name, ban.Ip, ban.Reason, ban.Expiration));
        }
    }
}
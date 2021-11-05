using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Bans
{
    public sealed class BanQueries : IBanQueries
    {
        private readonly IBanManager _banRepository;

        public BanQueries(IBanManager banRepository)
        {
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<BanViewModel?> GetAsync(PlayerId id, CancellationToken ct)
        {
            return await _banRepository.GetAsync(id, ct) is { } ban ? new BanViewModel(ban.BannerId, ban.Id, ban.Ip, ban.Reason, ban.Expiration) : null;
        }

        public IAsyncEnumerable<BanViewModel> GetAllAsync(CancellationToken ct)
        {
            return _banRepository.GetAllAsync(ct).Select(ban => new BanViewModel(ban.BannerId, ban.Id, ban.Ip, ban.Reason, ban.Expiration));
        }
    }
}
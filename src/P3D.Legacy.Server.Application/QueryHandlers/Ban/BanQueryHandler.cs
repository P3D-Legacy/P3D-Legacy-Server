using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Application.Queries.Ban;
using P3D.Legacy.Server.CQERS.Queries;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Ban
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class BanQueryHandler :
        IQueryHandler<GetPlayerBanQuery, BanViewModel?>,
        IQueryHandler<GetPlayerBansQuery, ImmutableArray<BanViewModel>>
    {
        private readonly IBanRepository _banRepository;

        public BanQueryHandler(IBanRepository banRepository)
        {
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<BanViewModel?> HandleAsync(GetPlayerBanQuery query, CancellationToken ct)
        {
            var id = query.Id;

            return await _banRepository.GetAsync(id, ct) is { } ban ? new BanViewModel(ban.BannerId, ban.Id, ban.Ip, ban.Reason, ban.Expiration) : null;
        }

        public async Task<ImmutableArray<BanViewModel>> HandleAsync(GetPlayerBansQuery query, CancellationToken ct)
        {
            return await _banRepository.GetAllAsync(ct).Select(static x => new BanViewModel(x.BannerId, x.Id, x.Ip, x.Reason, x.Expiration)).ToImmutableArrayAsync(ct);
        }
    }
}
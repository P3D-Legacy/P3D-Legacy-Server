using P3D.Legacy.Server.Abstractions.Queries;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Application.Queries.Ban;
using P3D.Legacy.Server.Infrastructure.Services.Bans;

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
        private readonly IBanManager _banRepository;

        public BanQueryHandler(IBanManager banRepository)
        {
            _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
        }

        public async Task<BanViewModel?> Handle(GetPlayerBanQuery request, CancellationToken ct)
        {
            return await _banRepository.GetAsync(request.Id, ct) is { } ban ? new BanViewModel(ban.BannerId, ban.Id, ban.Ip, ban.Reason, ban.Expiration) : null;
        }

        public async Task<ImmutableArray<BanViewModel>> Handle(GetPlayerBansQuery request, CancellationToken ct)
        {
            return await _banRepository.GetAllAsync(ct).Select(ban => new BanViewModel(ban.BannerId, ban.Id, ban.Ip, ban.Reason, ban.Expiration)).ToImmutableArrayAsync(ct);
        }
    }
}
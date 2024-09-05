using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.Application.Queries.Ban;
using P3D.Legacy.Server.CQERS.Queries;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Ban;

internal sealed class BanQueryHandler : IQueryHandler<GetPlayerBanQuery, BanViewModel?>
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
}
using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Ban;
using P3D.Legacy.Server.Domain.Repositories;

using System;
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
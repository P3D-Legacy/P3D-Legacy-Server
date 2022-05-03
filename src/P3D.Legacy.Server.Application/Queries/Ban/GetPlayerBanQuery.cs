using P3D.Legacy.Common;
using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.Ban
{
    public sealed record GetPlayerBanQuery(PlayerId Id) : IQuery<BanViewModel?>;
}
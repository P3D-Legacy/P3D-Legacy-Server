using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Immutable;

namespace P3D.Legacy.Server.Application.Queries.Ban
{
    public sealed record GetPlayerBansQuery : IQuery<ImmutableArray<BanViewModel>>;
}
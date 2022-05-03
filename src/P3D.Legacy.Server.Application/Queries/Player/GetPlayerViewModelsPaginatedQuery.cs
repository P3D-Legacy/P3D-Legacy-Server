using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Immutable;

namespace P3D.Legacy.Server.Application.Queries.Player
{
    public sealed record GetPlayerViewModelsPaginatedQuery(int Skip, int Take) : IQuery<(long Count, ImmutableArray<PlayerViewModel> Models)>;
}
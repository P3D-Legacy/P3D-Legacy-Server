using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.Player
{
    public sealed record GetPlayerViewModelQuery(long Origin) : IQuery<PlayerViewModel?>;
}
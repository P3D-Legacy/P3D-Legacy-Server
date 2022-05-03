using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Immutable;

namespace P3D.Legacy.Server.Application.Queries.Player
{
    public sealed record GetPlayersInitializedQuery : IQuery<ImmutableArray<IPlayer>>;
}
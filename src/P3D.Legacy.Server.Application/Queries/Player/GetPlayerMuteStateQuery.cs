using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Queries;

namespace P3D.Legacy.Server.Application.Queries.Player
{
    public sealed record GetPlayerMuteStateQuery(PlayerId PlayerId, PlayerId TargetPlayerId) : IQuery<bool>;
}
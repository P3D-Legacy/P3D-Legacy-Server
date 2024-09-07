using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Queries.Player;

public sealed record GetPlayerMuteStateQuery(PlayerId PlayerId, PlayerId TargetPlayerId) : IQuery<bool>;
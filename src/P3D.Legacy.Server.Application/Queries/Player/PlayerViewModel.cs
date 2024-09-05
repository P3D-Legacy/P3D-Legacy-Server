using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Application.Queries.Player;

public sealed record PlayerViewModel(long Id, string Name, GameJoltId GameJoltId);
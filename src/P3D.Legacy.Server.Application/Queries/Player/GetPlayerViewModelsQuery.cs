using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Immutable;

namespace P3D.Legacy.Server.Application.Queries.Player;

public sealed record GetPlayerViewModelsQuery : IQuery<ImmutableArray<PlayerViewModel>>;
using System.Collections.Immutable;

namespace P3D.Legacy.Server.Domain.Queries.Player;

public sealed record GetPlayerViewModelsPaginatedQuery(int Skip, int Take) : IQuery<(long Count, ImmutableArray<PlayerViewModel> Models)>;
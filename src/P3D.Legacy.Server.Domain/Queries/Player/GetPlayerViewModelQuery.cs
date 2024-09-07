namespace P3D.Legacy.Server.Domain.Queries.Player;

public sealed record GetPlayerViewModelQuery(long Origin) : IQuery<PlayerViewModel?>;
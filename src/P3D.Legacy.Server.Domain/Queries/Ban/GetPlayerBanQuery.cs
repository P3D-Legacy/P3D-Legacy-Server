using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Queries.Ban;

public sealed record GetPlayerBanQuery(PlayerId Id) : IQuery<BanViewModel?>;
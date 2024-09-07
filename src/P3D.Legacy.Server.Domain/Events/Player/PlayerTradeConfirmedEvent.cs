using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerTradeConfirmedEvent(IPlayer Player, Origin Partner) : IEvent;
namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerUpdatedPositionEvent(IPlayer Player) : IEvent;
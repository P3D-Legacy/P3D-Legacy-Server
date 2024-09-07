namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerUpdatedStateEvent(IPlayer Player) : IEvent;
namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerJoinedEvent(IPlayer Player) : IEvent
{
    public override string ToString() => $"{Player.Name} joined.";
}
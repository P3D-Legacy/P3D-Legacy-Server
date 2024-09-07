using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerLeftEvent(PlayerId Id, Origin Origin, string Name) : IEvent
{
    public override string ToString() => $"{Name} left.";
}
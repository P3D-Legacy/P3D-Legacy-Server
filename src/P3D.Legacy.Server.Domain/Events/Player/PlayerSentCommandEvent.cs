namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerSentCommandEvent(IPlayer Player, string Command) : IEvent;
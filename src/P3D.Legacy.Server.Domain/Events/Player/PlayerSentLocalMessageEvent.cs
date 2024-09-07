namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerSentLocalMessageEvent(IPlayer Player, string Location, string Message) : IEvent;
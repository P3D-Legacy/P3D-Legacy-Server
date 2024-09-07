namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerSentGlobalMessageEvent(IPlayer Player, string Message) : IEvent;
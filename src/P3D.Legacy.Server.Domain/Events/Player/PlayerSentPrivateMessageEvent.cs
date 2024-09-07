namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerSentPrivateMessageEvent(IPlayer Player, string ReceiverName, string Message) : IEvent;
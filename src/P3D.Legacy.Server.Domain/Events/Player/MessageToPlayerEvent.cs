namespace P3D.Legacy.Server.Domain.Events.Player;

public record MessageToPlayerEvent(IPlayer From, IPlayer To, string Message) : IEvent;
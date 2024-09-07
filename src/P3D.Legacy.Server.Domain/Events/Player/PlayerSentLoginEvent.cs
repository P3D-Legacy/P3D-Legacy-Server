namespace P3D.Legacy.Server.Domain.Events.Player;

public sealed record PlayerSentLoginEvent(IPlayer Player, string Password) : IEvent;
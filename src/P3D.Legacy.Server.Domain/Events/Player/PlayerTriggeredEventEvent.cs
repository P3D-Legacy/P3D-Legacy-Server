using P3D.Legacy.Common.PlayerEvents;

namespace P3D.Legacy.Server.Domain.Events.Player;

public record PlayerTriggeredEventEvent(IPlayer Player, PlayerEvent Event) : IEvent;
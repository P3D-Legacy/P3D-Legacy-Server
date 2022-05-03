using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public record PlayerTriggeredEventEvent(IPlayer Player, PlayerEvent Event) : IEvent;
}
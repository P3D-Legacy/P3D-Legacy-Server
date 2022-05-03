using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public record WorldUpdatedEvent(WorldState State, WorldState OldState) : IEvent;
}
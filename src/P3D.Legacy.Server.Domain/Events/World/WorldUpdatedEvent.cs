using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Server.Domain.Events.World;

public record WorldUpdatedEvent(WorldState State, WorldState OldState) : IEvent;
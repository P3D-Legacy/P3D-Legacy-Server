using MediatR;

using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record WorldUpdatedNotification(WorldState State, WorldState OldState) : INotification;
}
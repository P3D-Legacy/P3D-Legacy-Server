using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerUpdatedStateNotification(IPlayer Player) : INotification;
}
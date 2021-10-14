using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Notifications
{
    public sealed record PlayerJoinedNotification(IPlayer Player) : INotification;
}
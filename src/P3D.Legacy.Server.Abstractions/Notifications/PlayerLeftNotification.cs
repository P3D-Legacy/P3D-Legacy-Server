using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerLeftNotification(PlayerId Id, Origin Origin, string Name) : INotification;
}
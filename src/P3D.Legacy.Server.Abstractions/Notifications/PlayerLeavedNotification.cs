using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerLeavedNotification(long Id, string Name, GameJoltId GameJoltId) : INotification;
}
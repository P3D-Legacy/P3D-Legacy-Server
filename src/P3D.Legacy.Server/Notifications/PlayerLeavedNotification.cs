using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerLeavedNotification(long Id, string Name, ulong GameJoltId) : INotification;
}
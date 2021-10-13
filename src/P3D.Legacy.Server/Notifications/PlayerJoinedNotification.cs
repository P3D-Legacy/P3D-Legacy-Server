using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerJoinedNotification(long Id, string Name, ulong GameJoltId) : INotification;
}
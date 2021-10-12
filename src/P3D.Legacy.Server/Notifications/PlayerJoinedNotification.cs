using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerJoinedNotification(ulong Id, string Name, ulong GameJoltId) : INotification;
}
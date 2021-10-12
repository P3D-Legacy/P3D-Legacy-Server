using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerLeavedNotification(ulong Id, string Name, ulong GameJoltId) : INotification;
}
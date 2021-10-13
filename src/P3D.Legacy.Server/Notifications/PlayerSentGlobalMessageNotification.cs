using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentGlobalMessageNotification(long Id, string Name, ulong GameJoltId, string Message) : INotification;
}
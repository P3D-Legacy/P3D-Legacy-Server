using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentLocalMessageNotification(ulong Id, string Name, ulong GameJoltId, string Location, string Message) : INotification;
}
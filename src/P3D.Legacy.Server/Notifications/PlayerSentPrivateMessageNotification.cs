using MediatR;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentPrivateMessageNotification(long Id, string Name, ulong GameJoltId, string ReceiverName, string Message) : INotification;
}
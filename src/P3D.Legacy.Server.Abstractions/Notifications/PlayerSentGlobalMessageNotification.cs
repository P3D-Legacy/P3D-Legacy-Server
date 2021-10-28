using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentGlobalMessageNotification(IPlayer Player, string Message) : INotification;
}
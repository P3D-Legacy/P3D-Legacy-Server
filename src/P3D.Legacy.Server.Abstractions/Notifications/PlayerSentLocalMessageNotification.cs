using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentLocalMessageNotification(IPlayer Player, string Location, string Message) : INotification;
}
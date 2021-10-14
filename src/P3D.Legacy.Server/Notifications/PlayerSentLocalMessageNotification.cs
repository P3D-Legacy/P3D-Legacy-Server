using MediatR;

using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentLocalMessageNotification(IPlayer Player, string Location, string Message) : INotification;
}
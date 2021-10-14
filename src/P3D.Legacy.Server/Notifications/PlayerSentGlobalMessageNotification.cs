using MediatR;

using P3D.Legacy.Server.Models;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentGlobalMessageNotification(IPlayer Player, string Message) : INotification;
}
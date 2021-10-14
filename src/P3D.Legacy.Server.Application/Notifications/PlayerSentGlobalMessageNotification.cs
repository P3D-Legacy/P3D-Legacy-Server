using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Notifications
{
    public sealed record PlayerSentGlobalMessageNotification(IPlayer Player, string Message) : INotification;
}
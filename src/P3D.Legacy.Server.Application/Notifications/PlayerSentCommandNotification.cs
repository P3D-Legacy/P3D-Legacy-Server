using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Notifications
{
    public sealed record PlayerSentCommandNotification(IPlayer Player, string Command) : INotification;
}
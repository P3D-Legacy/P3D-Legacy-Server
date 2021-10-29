using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentLoginNotification(IPlayer Player, string Password) : INotification;
}
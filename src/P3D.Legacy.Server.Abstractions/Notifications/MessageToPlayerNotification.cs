using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record MessageToPlayerNotification(IPlayer From, IPlayer To, string Message) : INotification;
}
using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Notifications
{
    public record MessageToPlayerNotification(IPlayer From, IPlayer To, string Message) : INotification;
}
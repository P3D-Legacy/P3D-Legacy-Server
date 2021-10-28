using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record PlayerTriggeredEventNotification(IPlayer Player, string EventMessage) : INotification;
}
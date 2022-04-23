using MediatR;

using P3D.Legacy.Common.Events;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record PlayerTriggeredEventNotification(IPlayer Player, Event Event) : INotification;
}
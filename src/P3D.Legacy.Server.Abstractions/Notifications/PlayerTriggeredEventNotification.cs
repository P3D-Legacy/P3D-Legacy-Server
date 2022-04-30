using P3D.Legacy.Common.PlayerEvents;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record PlayerTriggeredEventNotification(IPlayer Player, PlayerEvent Event) : INotification;
}
using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerTradeInitiatedNotification(IPlayer Initiator, Origin Target) : INotification;
}
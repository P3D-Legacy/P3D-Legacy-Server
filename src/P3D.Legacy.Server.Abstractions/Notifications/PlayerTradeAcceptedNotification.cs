using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerTradeAcceptedNotification(IPlayer Target, Origin Initiator) : INotification;
}
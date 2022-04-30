namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerUpdatedPositionNotification(IPlayer Player) : INotification;
}
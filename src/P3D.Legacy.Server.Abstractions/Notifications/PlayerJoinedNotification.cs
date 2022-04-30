namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerJoinedNotification(IPlayer Player) : INotification;
}
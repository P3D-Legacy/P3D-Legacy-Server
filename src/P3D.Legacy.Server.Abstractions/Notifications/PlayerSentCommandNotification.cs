namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentCommandNotification(IPlayer Player, string Command) : INotification;
}
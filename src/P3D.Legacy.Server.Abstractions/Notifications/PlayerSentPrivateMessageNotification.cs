namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerSentPrivateMessageNotification(IPlayer Player, string ReceiverName, string Message) : INotification;
}
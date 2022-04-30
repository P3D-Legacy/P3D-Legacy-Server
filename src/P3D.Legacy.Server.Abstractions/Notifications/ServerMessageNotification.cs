namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record ServerMessageNotification(string Message) : INotification;
}
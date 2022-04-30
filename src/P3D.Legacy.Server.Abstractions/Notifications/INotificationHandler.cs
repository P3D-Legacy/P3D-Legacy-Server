namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public interface INotificationHandler<in TNotification> : MediatR.INotificationHandler<TNotification> where TNotification : INotification { }
}
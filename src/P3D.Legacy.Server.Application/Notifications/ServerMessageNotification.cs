using MediatR;

namespace P3D.Legacy.Server.Application.Notifications
{
    public record ServerMessageNotification(string Message) : INotification;
}
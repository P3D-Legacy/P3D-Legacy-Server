using MediatR;

namespace P3D.Legacy.Server.Application.Notifications
{
    public record WorldUpdatedNotification() : INotification;
}
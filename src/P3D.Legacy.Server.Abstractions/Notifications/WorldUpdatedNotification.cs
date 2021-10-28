using MediatR;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public record WorldUpdatedNotification() : INotification;
}
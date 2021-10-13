using MediatR;

using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerSentGameDataNotification(long Id, string Name, ulong GameJoltId, DataItemStorage GameData) : INotification;
}
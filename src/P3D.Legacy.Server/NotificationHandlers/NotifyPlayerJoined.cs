/*
using MediatR;

using P3D.Legacy.Server.Models.Events;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.NotificationHandlers
{
    public class NotifyPlayerJoined : INotificationHandler<PlayerJoinedNotification>
    {
        private readonly IPlayerContainerReader _playerContainer;

        public NotifyPlayerJoined(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            var (id, name, gameJoltId) = notification;

            await foreach (var player in _playerContainer.GetAllAsync(ct))
            {
                await player.NotifyAsync(new PlayerJoinedEvent(new EventPlayerModel(id, name, gameJoltId)), ct);
            }
        }
    }
}
*/
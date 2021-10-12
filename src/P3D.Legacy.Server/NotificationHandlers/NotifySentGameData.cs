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
    public class NotifySentGameData : INotificationHandler<PlayerSentGameDataNotification>
    {
        private readonly IPlayerContainerReader _playerContainer;

        public NotifySentGameData(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task Handle(PlayerSentGameDataNotification notification, CancellationToken ct)
        {
            var (id, name, gameJoltId, gameData) = notification;

            await foreach (var player in _playerContainer.GetAllAsync(ct))
            {
                await player.NotifyAsync(new PlayerGameDataEvent(new EventPlayerModel(id, name, gameJoltId), gameData), ct);
            }
        }
    }
}
*/
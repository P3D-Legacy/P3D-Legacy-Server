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
    public class NotifySentGlobalMessage : INotificationHandler<PlayerSentGlobalMessageNotification>
    {
        private readonly IPlayerContainerReader _playerContainer;

        public NotifySentGlobalMessage(IPlayerContainerReader playerContainer)
        {
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            var (id, name, gameJoltId, message) = notification;

            await foreach (var player in _playerContainer.GetAllAsync(ct))
            {
                await player.NotifyAsync(new PlayerGlobalMessageEvent(new EventPlayerModel(id, name, gameJoltId), message), ct);
            }
        }
    }
}
*/
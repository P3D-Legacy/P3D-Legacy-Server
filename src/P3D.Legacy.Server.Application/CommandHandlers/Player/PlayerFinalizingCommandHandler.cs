using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerFinalizingCommandHandler : IRequestHandler<PlayerFinalizingCommand>
    {
        private readonly ILogger _logger;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerFinalizingCommandHandler(ILogger<PlayerFinalizingCommandHandler> logger, NotificationPublisher notificationPublisher, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerFinalizingCommand request, CancellationToken ct)
        {
            var player = request.Player;

            if (await _playerContainer.RemoveAsync(player, ct))
            {
                if (!player.Id.IsEmpty)
                {
                    await _notificationPublisher.Publish(new PlayerLeftNotification(player.Id, player.Origin, player.Name), ct);
                }
            }

            return Unit.Value;
        }
    }
}
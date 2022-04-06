using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerReadyCommandHandler : IRequestHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly NotificationPublisher _notificationPublisher;
        private readonly IPermissionManager _permissionManager;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, NotificationPublisher notificationPublisher, IPermissionManager permissionManager, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            var permissions = await _permissionManager.GetByIdAsync(request.Player.Id, ct);
            await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);

            //await _playerContainer.AddAsync(request.Player, ct);

            await _notificationPublisher.Publish(new PlayerJoinedNotification(request.Player), ct);
            return Unit.Value;
        }
    }
}
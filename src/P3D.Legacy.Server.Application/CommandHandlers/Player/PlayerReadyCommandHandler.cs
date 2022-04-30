using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerReadyCommandHandler : ICommandHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IPermissionManager _permissionManager;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, INotificationDispatcher notificationDispatcher, IPermissionManager permissionManager, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<CommandResult> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            var permissions = await _permissionManager.GetByIdAsync(request.Player.Id, ct);
            await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);

            //await _playerContainer.AddAsync(request.Player, ct);

            await _notificationDispatcher.DispatchAsync(new PlayerJoinedNotification(request.Player), ct);
            return CommandResult.Success;
        }
    }
}
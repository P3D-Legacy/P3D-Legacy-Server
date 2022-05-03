using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;
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
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IPermissionManager _permissionManager;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IEventDispatcher eventDispatcher, IPermissionManager permissionManager, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<CommandResult> HandleAsync(PlayerReadyCommand command, CancellationToken ct)
        {
            var player = command.Player;

            var permissions = await _permissionManager.GetByIdAsync(player.Id, ct);
            await player.AssignPermissionsAsync(permissions.Permissions, ct);

            //await _playerContainer.AddAsync(request.Player, ct);

            await _eventDispatcher.DispatchAsync(new PlayerJoinedEvent(player), ct);
            return CommandResult.Success;
        }
    }
}
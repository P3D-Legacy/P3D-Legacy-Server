using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;

using System;
using System.Diagnostics;
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
        private readonly IPermissionRepository _permissionRepository;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IEventDispatcher eventDispatcher, IPermissionRepository permissionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        }

        public async Task<CommandResult> HandleAsync(PlayerReadyCommand command, CancellationToken ct)
        {
            var player = command.Player;

            Debug.Assert(player.State == PlayerState.Authentication);

            var permissions = await _permissionRepository.GetByIdAsync(player.Id, ct);
            await player.AssignPermissionsAsync(permissions.Permissions, ct);

            await _eventDispatcher.DispatchAsync(new PlayerJoinedEvent(player), ct);
            return CommandResult.Success;
        }
    }
}
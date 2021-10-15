using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Permissions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    public class PlayerReadyCommandHandler : IRequestHandler<PlayerReadyCommand>
    {
        private readonly ILogger _logger;
        private readonly IPlayerContainerWriter _playerContainer;
        private readonly IPermissionQueries _permissionQueries;

        public PlayerReadyCommandHandler(ILogger<PlayerReadyCommandHandler> logger, IPlayerContainerWriter playerContainer, IPermissionQueries permissionQueries)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
            _permissionQueries = permissionQueries ?? throw new ArgumentNullException(nameof(permissionQueries));
        }

        public async Task<Unit> Handle(PlayerReadyCommand request, CancellationToken ct)
        {
            var permissions = await _permissionQueries.GetByGameJoltAsync(request.Player.GameJoltId, ct);
            await request.Player.AssignPermissionsAsync(permissions.Permissions, ct);

            await _playerContainer.AddAsync(request.Player, ct);

            return Unit.Value;
        }
    }
}